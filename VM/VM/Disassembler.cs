#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using VMCore.VM.Core;
using VMCore.VM.Core.Utilities;
using VMCore.VM.Instructions;

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

        #endregion // Private Properties

        public Disassembler(VirtualMachine aVm)
        {
            Vm = aVm;
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
        public string[] Disassemble(bool aShowLocation = false,
                                    int aStartAddr = 0)
        {
            return 
                Disassemble(Vm.Cpu.MemExecutableSeqId,
                            aShowLocation,
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
        /// A string array containing one instruction per entry.
        /// </returns>
        public string[] Disassemble(int aMemSeqId,
                                    bool aShowLocation = false,
                                    int aStartAddr = 0)
        {
            // Reset the position of the stream back to
            // the start.
            var basePos = Vm.Memory.BaseMemorySize;
            var pos = aStartAddr + basePos;

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

            var minPos = region.Start;
            var maxPos = region.End;

            var subAddresses = new Dictionary<int, string>();
            var opCodes = new List<OpCode>();
            var instructions = new List<string>();
            var addresses = new List<int>();

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
                    addr =
                        $"{addresses[i]:X8} : ";
                }

                output[i] = addr + instructions[i];
            }

            return output;
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

            // The address is offset against the base position
            // of the executable memory region plus 8 for the
            // size of subroutine instruction plus the argument.
            if (aSubs.TryGetValue(memPtr + basePos + 8,
                                  out var subName))
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
            //Debug.WriteLine("Debugger: " + aPos);

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
                return $"???? {op:X2}";
            }

            // No instruction matching the OpCode was found.
            // In practice this shouldn't happen.
            if (!_instructionCache.TryGetValue(op,
                                               out var ins))
            {
                // Return the byte code as that's all we can
                // safely provide.
                aOp = OpCode.NOP;
                return $"???? {op:X2}";
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
