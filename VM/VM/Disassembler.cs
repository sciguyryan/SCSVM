﻿#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Emit;
using VMCore.VM.Core;
using VMCore.VM.Core.Utilities;
using VMCore.VM.Instructions;
using OpCode = VMCore.VM.Core.OpCode;

namespace VMCore.VM
{
    public class Disassembler
    {
        #region Public Properties

        public VirtualMachine Vm { get; }

        #endregion // Public Properties

        #region Private Properties

        /// <summary>
        /// A cached of the opcodes to their instruction instances.
        /// </summary>
        private readonly Dictionary<OpCode, Instruction> _instructionCache =
            ReflectionUtils.InstructionCache;

        /*private readonly Dictionary<OpCode, ConsoleColor> _highlights =
            new Dictionary<OpCode, ConsoleColor>()
            {
                { OpCode.MOV_HREG_PTR_REG, ConsoleColor.DarkGreen }
            };*/

        #endregion // Private Properties

        public Disassembler(VirtualMachine aVm)
        {
            Vm = aVm;
        }

        public void DisplayDisassembly(bool aShowLocation = false,
                                       bool aOffsetLocations = false,
                                       bool aHighlightLines = true,
                                       int aStartAddr = 0)
        {
            var (lines, opcodes) =
                Disassemble(aShowLocation, aOffsetLocations, aStartAddr);
            var len = lines.Length;

            for (var i = 0; i < len; i++)
            {
                /*if (aHighlightLines)
                {
                    if (_highlights.TryGetValue(opcodes[i],
                                                out var colour))
                    {
                        Console.Write(lines[i][..11]);
                        Console.ForegroundColor = colour;
                        Console.WriteLine(lines[i][11..]);
                    }
                    else
                    {
                        Console.ResetColor();
                        Console.WriteLine(lines[i]);
                    }
                }*/
                Console.WriteLine(lines[i]);
            }
        }

        /// <summary>
        /// Converts the byte code of a program back into assembly.
        /// This will skip any exceptions that would otherwise be thrown
        /// when executing this code. This method will use the memory
        /// sequence ID specified by the CPU.
        /// </summary>
        /// <param name="aShowLocation">
        /// If the binary locations of the commands should be shown.
        /// </param>
        /// <param name="aStartAddr">
        /// The address from which the execution should commence.
        /// </param>
        /// <returns>
        /// A string array containing one instruction per entry.
        /// </returns>
        public (string[], OpCode[]) Disassemble(bool aShowLocation = false,
                                                bool aOffsetLocations = false,
                                                int aStartAddr = 0)
        {
            return 
                Disassemble(Vm.Cpu.MemExecutableSeqId,
                            aShowLocation,
                            aOffsetLocations,
                            aStartAddr);
        }

        /// <summary>
        /// Converts the byte code of a program back into assembly.
        /// This will skip any exceptions that would otherwise be thrown
        /// when executing this code.
        /// </summary>
        /// <param name="aMemSeqId">
        /// The sequence ID for the memory region containing the code.
        /// </param>
        /// <param name="aShowLocation">
        /// If the binary locations of the commands should be shown.
        /// </param>
        /// <param name="aStartAddr">
        /// The address from which the execution should commence.
        /// </param>
        /// <returns>
        /// A tuple containing a list of the formatted strings
        /// giving one instruction per line and a list of the
        /// identified opcodes for each line.
        /// </returns>
        public (string[], OpCode[]) Disassemble(int aMemSeqId,
                                                bool aShowLocation = false,
                                                bool aOffsetLocations = false,
                                                int aStartAddr = 0)
        {
            var region =
                Vm.Memory.GetMemoryRegion(aMemSeqId);
            if (region is null)
            {
                throw new Exception
                (
                    "Disassemble: the specified memory sequence ID " +
                    $"{aMemSeqId} is invalid. No disassembly is possible."
                );
            }

            // Reset the position of the stream back to
            // the start.
            var minPos = region.Start;
            var baseInsPos = aStartAddr + minPos;
            var maxPos = region.End;

            var subAddresses = new Dictionary<int, string>();
            var opCodes = new List<OpCode>();
            var instructions = new List<string>();
            var addresses = new List<int>();

            var pos = baseInsPos;
            while (pos >= minPos && pos < maxPos)
            {
                addresses.Add(pos);

                var ins =
                    DisassembleNextInstruction(ref pos, out var op);
                if (op == OpCode.SUBROUTINE)
                {
                    // We have a subroutine.
                    // We do not want the colon at the
                    // end so we strip that away here.
                    subAddresses.Add(pos, ins[..^1]);
                }

                instructions.Add(ins);
                opCodes.Add(op);
            }

            // Do we need to make any adjustments to these instructions?
            var len = instructions.Count;
            for (var i = 0; i < len; i++)
            {
                switch (opCodes[i])
                {
                    case OpCode.CAL_LIT:
                        instructions[i] =
                            CleanCallInstruction(instructions[i],
                                                 subAddresses);
                        break;
                }
            }

            // Construct the full disassembled line.
            var output = new string[len];
            for (var i = 0; i < len; i++)
            {
                var addr = "";
                if (aShowLocation)
                {
                    var offsetPos = addresses[i];
                    if (aOffsetLocations)
                    {
                        offsetPos -= baseInsPos;
                    }

                    addr =
                        $"{offsetPos:X8} : ";
                }

                output[i] = addr + instructions[i];
            }

            return (output, opCodes.ToArray());
        }

        #region Clean Up Instruction Methods

        private string CleanCallInstruction(string aInstruction,
                                            IDictionary<int, string> aSubs)
        {
            var basePos = Vm.Memory.BaseMemorySize;

            var insStr = aInstruction;

            int memPtr;
            var offset = 7;

            // Are we dealing with a hex or normal
            // integer literal?
            if (insStr[offset..(offset + 2)] == "0x")
            {
                offset += 2;
                Utils.TryParseHexInt(insStr[offset..], out memPtr);
            }
            else
            {
                Utils.TryParseInt(insStr[offset..], out memPtr);
            }

            // 8 must be added to the position here to account for the
            // size of subroutine instruction plus the argument.
            if (aSubs.TryGetValue(memPtr + 8, out var subName))
            {
                return insStr[..5] + '!' + subName;
            }

            return insStr;
        }

        #endregion // Clean Up Instruction Methods

        /// <summary>
        /// Used to disassemble the next instruction.
        /// Essentially a clone of FetchExecuteNextInstruction
        /// but without the exception throwing code.
        /// </summary>
        /// <param name="aPos">
        /// The position in memory from which to begin 
        /// reading the instruction.
        /// </param>
        /// <param name="aOp">
        /// The identified opcode for the instruction if one was
        /// identified. NOP will be returned in the case of malformed
        /// data.
        /// </param>
        /// <returns>
        /// A string giving the disassembly of the next instruction.
        /// </returns>
        private string DisassembleNextInstruction(ref int aPos,
                                                  out OpCode aOp)
        {
            OpCode op;
            try
            {
                op =
                    Vm.Memory.GetOpCode(aPos,
                                        SecurityContext.System,
                                        true);
            }
            catch
            {
                aOp = OpCode.NOP;
                return string.Empty;
            }

            if (!Enum.IsDefined(typeof(OpCode), op))
            {
                // We do not recognize this opcode and so
                // we would have no meaningful output
                // here at all. Return the byte code instead.
                aOp = OpCode.NOP;
                return $"???? {(int)op:X2}";
            }

            // No instruction matching the OpCode was found.
            // In practice this shouldn't happen.
            if (!_instructionCache.TryGetValue(op,
                                               out var ins))
            {
                // Return the byte code as that's all we can
                // safely provide.
                aOp = OpCode.NOP;
                return $"???? {(int)op:X2}";
            }

            aPos += sizeof(OpCode);

            var opIns = new InstructionData
            {
                OpCode = op
            };

            aOp = op;

            // The types of the arguments expected for this instruction.
            var argTypes = ins.ArgumentTypes;

            // Iterate through the list of arguments and attempt
            // to populate the data.
            try
            {
                foreach (var t in argTypes)
                {
                    opIns.Args.Add(new InstructionArg
                    {
                        Value = 
                            Vm.Cpu.GetNextInstructionArgument(ref aPos, t)
                    });
                }
            }
            catch
            {
                // Do nothing.
            }

            if (opIns.Args.Count == ins.ArgumentTypes.Length)
            {
                return ins.ToString(opIns);
            }

            // If the number of arguments is not equal to the expected
            // number then the data is malformed.
            // Try to get the most information that we can
            // from this but the data is likely to be useless.

            var s = $"{ins.AsmName}";
            foreach (var arg in opIns.Args)
            {
                s += $" {arg:X2}";
            }

            return s;
        }
    }
}
