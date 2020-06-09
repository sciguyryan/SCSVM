#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using VMCore.Assembler.Optimizations;
using VMCore.VM.Core;
using VMCore.VM.Core.Utilities;
using VMCore.VM.Instructions;

namespace VMCore.Assembler
{
    public class Compiler
    {
        #region Private Properties

        /// <summary>
        /// The current version of the compiler.
        /// </summary>
        private readonly Version _compilerVersion 
            = new Version(0, 0, 1, 0);

        /// <summary>
        /// If we should attempt to Optimize certain
        /// instructions.
        /// </summary>
        private readonly bool _optimize;

        /// <summary>
        /// The binary writer used to write the binary data to the memory
        /// stream.
        /// </summary>
        private readonly BinaryWriter _bw;

        /// <summary>
        /// The memory stream into which the data will be written.
        /// </summary>
        private readonly MemoryStream _ms;

        /// <summary>
        /// A cached of the opcodes to their instruction instances.
        /// </summary>
        private readonly Dictionary<OpCode, Instruction> _instructionCache =
            ReflectionUtils.InstructionCache;

        /// <summary>
        /// A list of the labels to be replaced and their
        /// position within the data steam.
        /// </summary>
        private readonly Dictionary<string, long> _labelsToBeReplaced =
            new Dictionary<string, long>();

        /// <summary>
        /// A list of label destinations to be substituted within
        /// the data stream.
        /// </summary>
        private readonly Dictionary<string, long> _labelDestinations =
            new Dictionary<string, long>();

        /// <summary>
        /// A list of label constants to be substituted within
        /// the data stream.
        /// </summary>
        private readonly Dictionary<string, int> _labelConstants =
            new Dictionary<string, int>();

        /// <summary>
        /// The meta data to be added to the compiled binary file.
        /// </summary>
        private readonly BinMeta _fileMetaData;

        /// <summary>
        /// Section data to be used to compile the binary file.
        /// </summary>
        private readonly CompilerSections _sectionData;

        /// <summary>
        /// The address in which the program should be loaded into
        /// memory. Used to calculate label destinations.
        /// </summary>
        private const int InitialAddress = 64_400;

        /// <summary>
        /// The position within the file that gives the end
        /// of the meta data section.
        /// </summary>
        private int _metaSectionEnd;

        #endregion // Private Properties

        public Compiler(CompilerSections aSecs, BinMeta? aMeta, bool aOptimize)
        {
            _sectionData = aSecs;
            _fileMetaData = aMeta ?? new BinMeta
            {
                FileVersion = new Version("1.0.0.0"),
                CompilerVersion = _compilerVersion,
                Id = Guid.NewGuid(),
            };
            _optimize = aOptimize;

            _ms = new MemoryStream();
            _bw = new BinaryWriter(_ms);
        }

        /// <summary>
        /// Compile a binary.
        /// </summary>
        /// <returns>
        /// A byte array containing the complete compiled binary file.
        /// </returns>
        public byte[] Compile()
        {
            // Ensure that we have instructions to compile.
            var instructions =
                _sectionData.CodeSectionData.ToArray();
            if (instructions.Length == 0)
            {
                throw new Exception("Compile: no data to compile");
            }

            var sectionData = new List<SectionInfo>();

            // First we need to write the magic number
            // for our file format to the stream.
            _bw.Write(BinFile.MagicNumber);

            /*------------------ SECTION INFO POINTER ------------------*/
            // We can now calculate the position of the section
            // info pointer. For now we write a dummy value that
            // will be updated later.
            var secInfoPointerPos = (int)_ms.Position;
            _bw.Write(0);

            /*------------------ INITIAL OFFSET ------------------*/
            // This will indicate where the program should be loaded in memory.
            // without this we cannot correctly calculate label offsets.
            //const int initialAddress = 64_400;
            _bw.Write(InitialAddress);

            /*------------------ META DATA SECTION ------------------*/
            // Now we need to write the meta data section.
            var meta = _fileMetaData.Serialize();

            var metaStart = (int)_ms.Position;
            _bw.Write(meta);
            _metaSectionEnd = (int)_ms.Position;

            // Create the meta block.
            var metaSec = 
                new SectionInfo(BinSections.Meta,
                                metaStart,
                                _metaSectionEnd - metaStart);
            sectionData.Add(metaSec);

            // We will always have at least three sections:
            // * meta data section
            // * the byte code section
            // * the section data section
            var sectionCount = 2;


            /*------------------ CODE DATA SECTION ------------------*/
            var codeStart = (int)_ms.Position;
            foreach (var ins in instructions)
            {
                if (ins is null)
                {
                    continue;
                }

                // We need to deduct metaEnd from the position to
                // correctly offset the data.
                // Once we load the binary file into the virtual
                // machine byte 0 will be the first byte in
                // memory. If we leave the offsets as they are
                // they will point to the wrong place.
                // For example if we have a label pointing to 100
                // in the file. Once we disregard the meta and header
                // sections then the instruction will actually be
                // at position 100 - 56 = 44. (the size of the header
                // plus meta section).
                // If the pointer was left in it's original state
                // then it would point to the wrong place.
                AddWithLabel(ins.Op, ins.Args, ins.Label);
            }
            var codeEnd = (int)_ms.Position;

            // We can calculate the length of the instruction
            // binary data.
            var insLen = codeEnd - codeStart;

            // Create the section information block.
            var codeSec =
                new SectionInfo(BinSections.Text,
                                codeStart,
                                insLen);
            sectionData.Add(codeSec);


            /*------------------ DIRECTIVE DATA SECTION ------------------*/

            // Next we need to write the entries in the
            // data section. These are hard-coded data entries.
            // We need to do this before label substitution
            // takes place as leveraging that system
            // will make thing simpler to work with.
            var directives =
                _sectionData.DataSectionData.ToArray();
            if (directives.Length > 0)
            {
                ++sectionCount;
            }

            var dirStart = (int)_ms.Position;
            foreach (var dir in directives)
            {
                switch (dir.DirCode)
                {
                    // Defined byte code sequences.
                    case DirectiveCodes.DB:
                        AddAddressRefLabel(dir.DirLabel);
                        _bw.Write(dir.ByteData);
                        break;

                    // Constants.
                    case DirectiveCodes.EQU:
                        HandleEquDirective(dir);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            var dirEnd = (int)_ms.Position;

            // Create the section information block.
            var dataSec =
                new SectionInfo(BinSections.Data,
                                dirStart,
                                dirEnd - dirStart);
            sectionData.Add(dataSec);

            // Next we need to apply label substitutions.
            ReplaceLabels();


            /*------------------ SECTION INFO DATA ------------------*/
            ++sectionCount;

            var secInfoStart = (int)_ms.Position;
            _bw.Write(sectionCount);

            foreach (var sec in sectionData)
            {
                // Section ID is written first.
                _bw.Write((int)sec.SectionId);

                // Next we write the start position of the block.
                _bw.Write(sec.StartPosition);

                // Next we write the length of the block.
                _bw.Write(sec.Length);
            }

            var secInfoEnd = (int)_ms.Position + sizeof(int) * 3;

            // Now we write the section information entry... for this section!
            _bw.Write((int)BinSections.SectionInfoData);
            _bw.Write(secInfoStart);
            _bw.Write(secInfoEnd - secInfoStart);

            // Finally we can go back and update the section
            // info pointer to the starting point of this block.
            _ms.Position = secInfoPointerPos;
            _bw.Write(secInfoStart);

            // Restore the stream to the original location.
            _ms.Position = secInfoEnd;

            // Return the compiled data.
            var bytes = _ms.ToArray();

            // Clean up.
            _bw.Close();
            _ms.Close();

            _bw.Dispose();
            _ms.Dispose();

            return bytes;
        }

        public void AddWithLabel(OpCode aOpCode,
                                 object[]? aArgs,
                                 AsmLabel? aBoundLabel)
        {
            var args = aArgs ?? new object[0];

            if (!_instructionCache.TryGetValue(aOpCode,
                                               out var ins))
            {
                throw new InvalidDataException
                (
                    "AddWithLabel: attempted write an invalid " +
                    $"opcode with ID = {(int)aOpCode} to the " +
                    $"data stream at position {_ms.Position}."
                );
            }

            var op = aOpCode;
            if (op == OpCode.LABEL ||
                op == OpCode.SUBROUTINE)
            {
                var argIdx = op == OpCode.LABEL ? 0 : 1;
                AddAddressRefLabel((string)args[argIdx]);

                // We do not want to actually do anything
                // with label opcodes as they are placeholders only.
                // So we should just return here.
                if (op == OpCode.LABEL)
                {
                    return;
                }
            }

            // Quick exit, no argument data to write.
            if (ins.ArgumentTypes.Length == 0)
            {
                _bw.Write((int)op);
                return;
            }

            // We should have at least one argument here...
            if (args.Length < ins.ArgumentTypes.Length)
            {
                // TODO - handle this better.
                return;
            }

            // We have argument data to write.
            // For now just write a dummy no-op instruction
            // to the data stream.
            // This will be overwritten below.
            var opCodePos = _ms.Position;
            _bw.Write((int)op);

            var newOp = aOpCode;
            var hasOpCodeChanged = false;
            for (var i = 0; i < args.Length; i++)
            {
                // Subroutines are quirky.
                // We only care about their ID, however a second
                // argument will be passed via the parser. We do
                // not need to write this value to the binary file
                // as it is only needed for processing. Sigh...
                if (op == OpCode.SUBROUTINE && i > 0)
                {
                    break;
                }

                var argType = ins.ArgumentTypes[i];
                var arg = args[i];

                if (_optimize)
                {
                    (newOp, argType, arg)
                        = Optimize(newOp, i, ins, args[i]);

                    // We cannot change the opcode more than
                    // once during optimization otherwise it
                    // would likely cause things to break.
                    // In theory this should never happen.
                    if (newOp != aOpCode)
                    {
                        if (hasOpCodeChanged)
                        {
                            throw new NotSupportedException
                            (
                                "AddWithLabel: attempted to change " +
                                $"the opcode from {aOpCode} to " +
                                $"{newOp}, however the opcode has " +
                                "already been changed. This operation " +
                                "is not supported."
                            );
                        }
                    }

                    op = newOp;
                }

                // Check if we have a label bound to this
                // argument.
                if (!(aBoundLabel is null) &&
                    aBoundLabel.BoundArgumentIndex == i)
                {
                    // Do we know about this label already?
                    if (!_labelDestinations.TryGetValue(aBoundLabel.Name,
                                                        out var addr))
                    {
                        // No, we will have to replace it later.
                        _labelsToBeReplaced.Add(aBoundLabel.Name,
                                                _ms.Position);
                    }
                    else
                    {
                        // Yes, we can replace it immediately.
                        arg = (int)addr;
                    }
                }
                
                // Write the argument data.
                Utils.WriteDataByType(argType, arg, _bw);
            }

            if (!hasOpCodeChanged)
            {
                return;
            }

            // This is a little bit ugly.
            // Now that the instruction data has been
            // written we need to go back and write the opcode.
            // This is done last as it could have
            // changed during optimization.
            // After we are done restore the stream
            // to the correct position.
            var currPos = _ms.Position;
            _ms.Position = opCodePos;
            _bw.Write((int) op);
            _ms.Position = currPos;
        }

        /// <summary>
        /// Apply any label substitutions that have been applied.
        /// </summary>
        public void ReplaceLabels()
        {
            while (_labelsToBeReplaced.Count > 0)
            {
                var (name, addr)
                    = _labelsToBeReplaced.First();

                ReplaceLabel(name, addr);
            }
        }

        /// <summary>
        /// Handle the processing of an EQU compiler directive.
        /// </summary>
        /// <param name="aDir">
        /// The CompilerDir object holding the data.
        /// </param>
        private void HandleEquDirective(CompilerDir aDir)
        {
            if (aDir.StringData == "$")
            {
                // This simply points to the current
                // address.
                AddAddressRefLabel(aDir.DirLabel);
                return;
            }

            if (aDir.StringData.Length > 3 &&
                aDir.StringData[..2] == "$-")
            {
                // This is a label offset address.
                CompilerDir? refDir = null;
                foreach (var dir in _sectionData.DataSectionData)
                {
                    if (dir.DirLabel != aDir.StringData[2..])
                    {
                        continue;
                    }

                    refDir = dir;
                    break;
                }

                // We did not find a matching compiler directive
                // label.
                if (refDir is null)
                {
                    throw new InvalidDataException();
                }

                // Set the constant to be the value of the
                // length of the destination labels data.
                AddConstantLabel(aDir.DirLabel,
                                 refDir.ByteData.Length);
            }

            // If all else fails, is this a valid integer?
            if (!int.TryParse(aDir.StringData, out var result))
            {
                throw new InvalidDataException();
            }

            AddConstantLabel(aDir.DirLabel, result);
        }

        /// <summary>
        /// Add an address reference label.
        /// </summary>
        /// <param name="aLabel">The name of the label.</param>
        private void AddAddressRefLabel(string aLabel)
        {
            if (!_labelDestinations.TryAdd(aLabel, _ms.Position))
            {
                throw new InvalidDataException
                (
                    "AddAddressRefLabel: attempted to add label " +
                    $"'{aLabel}' at position " +
                    $"{_ms.Position} but a label " +
                    "with that name already exists."
                );
            }
        }

        /// <summary>
        /// Add a constant value label.
        /// </summary>
        /// <param name="aLabel">The name of the label.</param>
        /// <param name="aValue">The value of the label.</param>
        private void AddConstantLabel(string aLabel, int aValue)
        {
            if (!_labelConstants.TryAdd(aLabel, aValue))
            {
                throw new InvalidDataException
                (
                    "AddConstantLabel: attempted to add label " +
                    $"'{aLabel}' with value {aValue} but a label " +
                    "with that name already exists."
                );
            }
        }

        /// <summary>
        /// Optimize any operations that support optimization.
        /// </summary>
        /// <param name="aOp">
        /// The opcode of the instruction to be Optimized.
        /// </param>
        /// <param name="aArgIndex">
        /// The index of the argument to be Optimized.
        /// </param>
        /// <param name="aIns">
        /// The instruction instance for the opcode.
        /// </param>
        /// <param name="aArg">
        /// The data for the opcode argument.
        /// </param>
        /// <returns>
        /// A tuple of the output opcode, argument type and argument data.
        /// </returns>
        private (OpCode, Type, object) Optimize(OpCode aOp,
                                                int aArgIndex,
                                                Instruction aIns,
                                                object aArg)
        {

            Type argType = aIns.ArgumentTypes[aArgIndex];

            if (!_optimize)
            {
                return (aOp, argType, aArg);
            }

            // If this argument is an expression then we can check to
            // see if it is possible to fold it into a single value.
            // This will increase performance later as running
            // the expression parser within the CPU is more performance
            // intensive.
            if (aIns.ExpressionArgType(aArgIndex) != null)
            {
                return
                    FoldExpressionArg.FoldExpression(aOp,
                                                     aArgIndex,
                                                     aIns,
                                                     aArg);
            }

            return (aOp, argType, aArg);
        }

        /// <summary>
        /// Attempt to replace a single label.
        /// </summary>
        /// <param name="aLabelName">
        /// The name of the label to be replaced.
        /// </param>
        /// <param name="aOrigAddress">
        /// The address giving the position of the label to be
        /// replaced within the data stream.
        /// </param>
        private void ReplaceLabel(string aLabelName, long aOrigAddress)
        {
            byte[] bytes;

            if (_labelConstants.ContainsKey(aLabelName))
            {
                // This was a valid constant label.
                bytes = ConstantLabelData(aLabelName);
            }
            else if (_labelDestinations.ContainsKey(aLabelName))
            {
                // This was a destination address label.
                bytes = AddressLabelData(aLabelName);
            }
            else
            {
                // We did not find a valid match in either the constant
                // or position label lists.
                throw new InvalidDataException
                (
                    "ReplaceLabel: attempted to bind a label that does " +
                    $"not exist. Label = '{aLabelName}'."
                );
            }

            // The label has a matching destination.
            // Set the location of the stream to be the position
            // of the bytes corresponding to the location of
            // the label.
            var startPos = _ms.Position;
            _ms.Position = aOrigAddress;

            // Write out the new jump location to the stream.
            _bw.Write(bytes);

            // Restore the stream position.
            _ms.Position = startPos;

            // Remove the entry so we do not attempt to replace it again.
            _labelsToBeReplaced.Remove(aLabelName);
        }

        /// <summary>
        /// Get the bytes representing the value of a constant label.
        /// </summary>
        /// <param name="aLabelName">
        /// the name of the label.
        /// </param>
        /// <returns>
        /// An array of bytes representing the value of the label.
        /// </returns>
        private byte[] ConstantLabelData(string aLabelName)
        {
            return 
                BitConverter.GetBytes(_labelConstants[aLabelName]);
        }

        /// <summary>
        /// Get the bytes representing the destination address of
        /// an address-type label.
        /// </summary>
        /// <param name="aLabelName">
        /// the name of the label.
        /// </param>
        /// <returns>
        /// An array of bytes representing the destination
        /// address as specified by the label.
        /// </returns>
        private byte[] AddressLabelData(string aLabelName)
        {
            // First we subtract the address donating the end of the
            // meta section from the position.
            // If a label would point to the first byte of the
            // code section then it would now point to position zero
            // giving us a relative address.
            // Then we add the initial address that the binary will
            // have when loaded into memory. This will give us an
            // absolute address.
            var newAddress =
                ((int)_labelDestinations[aLabelName] - _metaSectionEnd) +
                InitialAddress;

            // TODO - check if this is Endian variable compatible.
            var union = new IntegerByteUnion()
            {
                integer = newAddress
            };

            return new[]
            {
                union.byte0,
                union.byte1,
                union.byte2,
                union.byte3
            };
        }
    }

    // A nice trick from here:
    // https://stackoverflow.com/questions/8827649/fastest-way-to-convert-int-to-4-bytes-in-c-sharp
    [StructLayout(LayoutKind.Explicit)]
    internal struct IntegerByteUnion
    {
        [FieldOffset(0)]
        public byte byte0;
        [FieldOffset(1)]
        public byte byte1;
        [FieldOffset(2)]
        public byte byte2;
        [FieldOffset(3)]
        public byte byte3;

        [FieldOffset(0)]
        public int integer;
    }
}
