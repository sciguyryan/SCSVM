using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using VMCore.Assembler.Optimisations;
using VMCore.VM;
using VMCore.VM.Core;

namespace VMCore.Assembler
{
    public class AsmWriter
    {
        /// <summary>
        /// If we should attempt to Optimize certain
        /// instructions.
        /// </summary>
        private bool _optimize = false;

        /// <summary>
        /// The binary writer for the data stream.
        /// </summary>
        private BinaryWriter _bw;

        /// <summary>
        /// The memory stream for the data stream.
        /// </summary>
        private MemoryStream _ms;

        /// <summary>
        /// A cached of the opcodes to their instruction instances.
        /// </summary>
        private Dictionary<OpCode, Instruction> _instructionCache = 
            ReflectionUtils.InstructionCache;

        /// <summary>
        /// A list of the labels to be replaced and their position within
        /// the data steam.
        /// </summary>
        private Dictionary<string, long> _labelsToBeReplaced = 
            new Dictionary<string, long>();

        /// <summary>
        /// A list of label destinations to be substituted within
        /// the data stream.
        /// </summary>
        private Dictionary<string, long> _labelDestinations = 
            new Dictionary<string, long>();

        public AsmWriter(bool optimize)
        {
            ReflectionUtils.BuildCachesAndHooks(true);

            _optimize = optimize;
            _ms = new MemoryStream();
            _bw = new BinaryWriter(_ms);
        }

        /// <summary>
        /// Add an opcode instruction to the byte stream.
        /// </summary>
        /// <param name="aOpCode">The opcode of the instruction.</param>
        public void Add(OpCode aOpCode)
        {
            AddWithLabel(aOpCode, null, null);
        }

        /// <summary>
        /// Add an opcode instruction to the byte stream.
        /// </summary>
        /// <param name="aOpCode">The opcode of the instruction.</param>
        /// <param name="aBoundLabel">A label that is bound to this opcode, can be null.</param>
        public void Add(OpCode aOpCode, AsmLabel aBoundLabel)
        {
            AddWithLabel(aOpCode, null, aBoundLabel);
        }

        /// <summary>
        /// Add an opcode instruction to the byte stream.
        /// </summary>
        /// <param name="aOpCode">The opcode of the instruction.</param>
        /// <param name="aArgs">Any argument data that is required by the opcode instruction.</param>
        public void Add(OpCode aOpCode, object[] aArgs)
        {
            AddWithLabel(aOpCode, aArgs, null);
        }

        /// <summary>
        /// Add an opcode instruction to the byte stream.
        /// </summary>
        /// <param name="aOpCode">The opcode of the instruction.</param>
        /// <param name="aArgs">Any argument data that is required by the opcode instruction.</param>
        /// <param name="aBoundLabel">A label that is bound to this opcode, can be null.</param>
        public void AddWithLabel(OpCode aOpCode,
                                 object[] aArgs,
                                 AsmLabel aBoundLabel)
        {
            if (!_instructionCache.TryGetValue(aOpCode,
                                               out Instruction ins))
            {
                // TODO - handle this better.
                return;
            }

            OpCode op = aOpCode;

            // Quick exit, no argument data to write.
            if (ins.ArgumentTypes.Length == 0)
            {
                _bw.Write((int)op);
                return;
            }

            // We should have at least one argument here...
            if (aArgs == null || aArgs.Length < ins.ArgumentTypes.Length)
            {
                // TODO - handle this better.
                return;
            }

            // We have arguments so for now
            // just write a dummy no-op instruction
            // to the data stream.
            // This will be changed below.
            var opCodePos = _bw.BaseStream.Position;
            _bw.Write(0);

            OpCode newOp = aOpCode;
            bool hasOpCodeChanged = false;
            for (int i = 0; i < aArgs.Length; i++)
            {
                Type argType = ins.ArgumentTypes[i];
                object arg = aArgs[i];

                if (_optimize)
                {
                    (newOp, argType, arg) 
                        = Optimize(newOp, i, ins, aArgs[i]);

                    // We cannot change the opcode more than
                    // once during optimization otherwise it
                    // would likely cause things to break.
                    // In theory this should never happen.
                    if (newOp != aOpCode)
                    {
                        if (hasOpCodeChanged)
                        {
                            throw new NotSupportedException($"AddWithLabel: attempted to change the opcode from {aOpCode} to {newOp}, however the opcode has already been changed. This operation is not supported.");
                        }
                    }

                    op = newOp;
                }

                // Check if we have a bound label.
                if (aBoundLabel != null)
                {
                    // Check if the instruction is permitted to bind a label
                    // to a given argument.
                    if (!ins.CanBindToLabel(i))
                    {
                        throw new Exception($"AddWithLabel: attempted to bind a label to an argument that cannot accept it. Op = {aOpCode}, boundLabel = {aBoundLabel.Name}, labelBoundArgument = {i}");
                    }

                    // TODO we can optimize here by replacing
                    // known labels immediately.
                    _labelsToBeReplaced.Add(aBoundLabel.Name,
                                            _ms.Position);
                }

                Utils.WriteDataByType(argType, arg, _bw);
            }

            // This is a little bit ugly.
            // Now that the instruction data has
            // been written we need to go back
            // and write the opcode.
            // This is done last as it could have
            // changed during optimization.
            // After we are done restore the stream
            // to the correct position.
            var currPos = _bw.BaseStream.Position;
            _bw.BaseStream.Position = opCodePos;
            _bw.Write((int)op);
            _bw.BaseStream.Position = currPos;
        }

        /// <summary>
        /// Optimize any operations that support optimization.
        /// </summary>
        /// <param name="aOp">The opcode of the instruction to be Optimized.</param>
        /// <param name="aArgIndex">The index of the argument to be Optimized.</param>
        /// <param name="aIns">The instruction instance for the opcode.</param>
        /// <param name="aArg">The data for the opcode argument.</param>
        /// <returns>A tuple of the output opcode, argument type and argument data.</returns>
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
        /// Create a label from a given ID.
        /// </summary>
        /// <param name="aId">The ID of this label.</param>
        /// <returns>A boolean, true if the label was added and false if the label already existed.</returns>
        public bool CreateLabel(string aId)
        {
            return _labelDestinations.TryAdd(aId, _ms.Position);
        }
        
        /// <summary>
        /// Apply any label index substitutions that have been applied.
        /// </summary>
        public void ApplyLabels()
        {
            foreach (var label in _labelsToBeReplaced)
            {
                // Check if the label has a matching destination.
                // If not then throw a compilation error as the resulting
                // binary is not valid.
                if (!_labelDestinations.ContainsKey(label.Key))
                {
                    throw new Exception($"ApplyLabels: attempted to bind a label that does not exist. Label = {label.Key}");
                }
                
                // TODO - check if this is Endian variable compatible.
                var union = new IntegerByteUnion()
                {
                    integer = (int)_labelDestinations[label.Key]
                };

                var bytes = new byte[]
                {
                    union.byte0,
                    union.byte1,
                    union.byte2,
                    union.byte3
                };

                // The label has a matching destination.
                // Set the location of the stream to be the position
                // of the bytes corresponding to the location of
                // the label.
                _ms.Position = label.Value;

                // Write out the new jump location to the stream.
                _bw.Write(bytes);
            }
        }

        /// <summary>
        /// Save the written data to a byte array.
        /// </summary>
        /// <returns>A byte array containing the compiled data.</returns>
        public byte[] Save()
        {
            ApplyLabels();

            return _ms.ToArray();
        }
    }

    // A nice trick from here:
    // https://stackoverflow.com/questions/8827649/fastest-way-to-convert-int-to-4-bytes-in-c-sharp
    [StructLayout(LayoutKind.Explicit)]
    struct IntegerByteUnion
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
