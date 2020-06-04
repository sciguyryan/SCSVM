﻿#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using VMCore.Assembler.Optimizations;
using VMCore.VM.Core;
using VMCore.VM.Core.Utilities;
using VMCore.VM.Instructions;

namespace VMCore.Assembler
{
    public class AsmWriter
    {
        /// <summary>
        /// If we should attempt to Optimize certain
        /// instructions.
        /// </summary>
        private readonly bool _optimize;

        /// <summary>
        /// The binary writer for the data stream.
        /// </summary>
        private readonly BinaryWriter _bw;

        /// <summary>
        /// The memory stream for the data stream.
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

        public AsmWriter(bool aOptimize)
        {
            _optimize = aOptimize;
            _ms = new MemoryStream();
            _bw = new BinaryWriter(_ms);
        }

        /// <summary>
        /// Add an opcode instruction to the byte stream.
        /// </summary>
        /// <param name="aOpCode">
        /// The opcode of the instruction.
        /// </param>
        public void Add(OpCode aOpCode)
        {
            AddWithLabel(aOpCode, null, null);
        }

        /// <summary>
        /// Add an opcode instruction to the byte stream.
        /// </summary>
        /// <param name="aOpCode">
        /// The opcode of the instruction.
        /// </param>
        /// <param name="aBoundLabel">
        /// A label that is bound to this opcode.
        /// Null if none has been specified.
        /// </param>
        public void Add(OpCode aOpCode, AsmLabel aBoundLabel)
        {
            AddWithLabel(aOpCode, null, aBoundLabel);
        }

        /// <summary>
        /// Add an opcode instruction to the byte stream.
        /// </summary>
        /// <param name="aOpCode">
        /// The opcode of the instruction.
        /// </param>
        /// <param name="aArgs">
        /// Any argument data that is required by the opcode instruction.
        /// </param>
        public void Add(OpCode aOpCode, object[] aArgs)
        {
            AddWithLabel(aOpCode, aArgs, null);
        }

        /// <summary>
        /// Add an opcode instruction to the byte stream.
        /// </summary>
        /// <param name="aOpCode">
        /// The opcode of the instruction.
        /// </param>
        /// <param name="aArgs">
        /// Any argument data that is required by the opcode instruction.
        /// </param>
        /// <param name="aBoundLabel">
        /// A label that is bound to this opcode, can be null.
        /// </param>
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
                    $"data stream at position {_bw.BaseStream.Position}."
                );
            }

            var op = aOpCode;
            if (op == OpCode.LABEL ||
                op == OpCode.SUBROUTINE)
            {
                var argIdx = op == OpCode.LABEL ? 0 : 1;
                var label = (string)args[argIdx];
                if (!_labelDestinations.TryAdd(label, _ms.Position))
                {
                    throw new InvalidDataException
                    (
                        "AddWithLabel: attempted to add label " +
                        $"'{args[0]}' at position " +
                        $"{_bw.BaseStream.Position} but a label " +
                        "with that name already exists."
                    );
                }

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
            var opCodePos = _bw.BaseStream.Position;
            _bw.Write(0);

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
                    // Check if the instruction is permitted to
                    // bind a label to this argument.
                    if (!ins.CanBindToLabel(i))
                    {
                        throw new ArgumentException
                        (
                            "AddWithLabel: attempted to bind a label " +
                            "to an argument that cannot accept it. " +
                            $"Op = {aOpCode}, boundLabel = '" +
                            $"{aBoundLabel.Name}', " +
                            $"argument ID = {i}"
                        );
                    }

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

                Utils.WriteDataByType(argType, arg, _bw);
            }

            // This is a little bit ugly.
            // Now that the instruction data has been
            // written we need to go back and write the opcode.
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
        /// Save the written data to a byte array.
        /// </summary>
        /// <returns>
        /// A byte array containing the compiled data.
        /// </returns>
        public byte[] Save()
        {
            ReplaceLabels();

            return _ms.ToArray();
        }

        /// <summary>
        /// Replace the address specified by a label with the
        /// corresponding destination address.
        /// </summary>
        /// <param name="aLabelName">
        /// The name of the label.
        /// </param>
        /// <param name="aOrigAddress">
        /// The origin address as specified by the label.
        /// </param>
        /// <exception cref="InvalidDataException">
        /// Thrown when a matching label cannot be found.
        /// </exception>
        private void ReplaceLabel(string aLabelName, long aOrigAddress)
        {
            // Check if the label has a matching destination.
            // If not then throw a compilation error as the resulting
            // binary is not valid.
            if (!_labelDestinations.ContainsKey(aLabelName))
            {
                throw new InvalidDataException
                (
                    "ReplaceLabel: attempted to bind a label that does " +
                    $"not exist. Label = '{aLabelName}'."
                );
            }

            // TODO - check if this is Endian variable compatible.
            var union = new IntegerByteUnion()
            {
                integer = (int)_labelDestinations[aLabelName]
            };

            var bytes = new []
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
            _ms.Position = aOrigAddress;

            // Write out the new jump location to the stream.
            _bw.Write(bytes);

            // Remove the entry so we do not attempt to replace it again.
            _labelsToBeReplaced.Remove(aLabelName);
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
