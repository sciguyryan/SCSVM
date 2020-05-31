#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using VMCore.VM.Core;
using VMCore.VM.Core.Exceptions;
using VMCore.VM.Core.Register;
using VMCore.VM.Core.Utilities;
using VMCore.VM.Instructions;

namespace VMCore.VM
{
    public class Cpu
    {
        #region Public Properties

        /// <summary>
        /// The memory region sequence ID that this CPU
        /// instance was assigned to begin executing.
        /// </summary>
        public int MemExecutableSeqId { get; private set; }

        /// <summary>
        /// A boolean indicating if the CPU is currently halted.
        /// </summary>
        public bool IsHalted { get; set; }

        /// <summary>
        /// The list of registers associated with this CPU instance.
        /// </summary>
        public RegisterCollection Registers { get; set; }

        /// <summary>
        /// The VM instance that holds this CPU.
        /// </summary>
        public VirtualMachine Vm { get; }

        #endregion // Public Properties

        #region Private Properties

        /// <summary>
        /// A cached of the opcodes to their instruction instances.
        /// </summary>
        /// <remarks>
        /// Since the CPU cannot be run on its own then this is safe
        /// to use here as the virtual machine parent will always
        /// have called the method to build these caches.
        /// </remarks>
        private readonly Dictionary<OpCode, Instruction> _instructionCache =
            ReflectionUtils.InstructionCache;

#if DEBUG
        private bool _isLoggingEnabled = true;
#else
        private bool _isLoggingEnabled = false;
#endif

        /// <summary>
        /// An internal indicator of if an IP breakpoint has been triggered.
        /// </summary>
        private bool _hasIpBreakpoint;

        /// <summary>
        /// An internal indicator of if a PC breakpoint has been triggered.
        /// </summary>
        private bool _hasPcBreakpoint;

        /// <summary>
        /// A shorthand for the user security context.
        /// </summary>
        private const SecurityContext UserCtx =
            SecurityContext.User;

        /// <summary>
        /// A short hand for the system security context.
        /// </summary>
        private const SecurityContext SysCtx =
            SecurityContext.System;

        /// <summary>
        /// A IP/user register tuple to avoid having to repeatedly create one.
        /// </summary>
        private readonly (Registers, SecurityContext) _ipUserTuple
            = (Core.Register.Registers.IP, UserCtx);

        /// <summary>
        /// A PC/system register tuple to avoid having to repeatedly create one.
        /// </summary>
        private readonly (Registers, SecurityContext) _pcSystemTuple
            = (Core.Register.Registers.PC, SysCtx);

        /// <summary>
        /// The lower memory bound from which data can be read or written
        /// within the program.
        /// </summary>
        private int _minExecutableBound = -1;

        /// <summary>
        /// The upper memory bound from which data can be read or written
        /// within the program.
        /// </summary>
        private int _maxExecutableBound = -1;

        /// <summary>
        /// If this CPU can swap between executable memory regions.
        /// </summary>
        private readonly bool _canSwapMemoryRegions;

        #endregion // Private Properties

        /// <summary>
        /// Create a new CPU instance.
        /// </summary>
        /// <param name="aVm">
        /// The virtual machine instance to which this CPU belongs.
        /// </param>
        /// <param name="aCanSwapMemoryRegions">
        /// A boolean, true if the CPU will be permitted to swap
        /// between executable memory regions, false otherwise.
        /// </param>
        public Cpu(VirtualMachine aVm,
                   bool aCanSwapMemoryRegions = false)
        {
            Vm = aVm;
            Registers = new RegisterCollection(this);

            _canSwapMemoryRegions = aCanSwapMemoryRegions;

            ResetStackPointer();
        }

        /// <summary>
        /// Initialize the CPU.
        /// </summary>
        /// <param name="aMemSeqId">
        /// The sequence ID for the memory region containing the code.
        /// </param>
        /// <param name="aStartAddr">
        /// The address from which the execution should commence.
        /// </param>
        public void Initialize(int aMemSeqId, int aStartAddr)
        {
            MemExecutableSeqId = aMemSeqId;

            if (_minExecutableBound != -1)
            {
                SetStartAddress(aStartAddr);
                SetBreakpointObservers();
                return;
            }

            if (_canSwapMemoryRegions)
            {
                _minExecutableBound = 0;
                _maxExecutableBound = Vm.Memory.Length;
            }
            else
            {
                var region =
                    Vm.Memory.GetMemoryRegion(aMemSeqId);
                if (region is null)
                {
                    throw new Exception
                    (
                        "Initialize: the specified memory sequence ID " +
                        $"{aMemSeqId} is invalid."
                    );
                }

                _minExecutableBound = region.Start;
                _maxExecutableBound = region.End;
            }

            SetStartAddress(aStartAddr);
            SetBreakpointObservers();
        }

        /// <summary>
        /// Execute a reset on the CPU. 
        /// Reset various registers and clear the halted state of the CPU.
        /// </summary>
        public void Reset()
        {
            // Reset the instruction pointer.
            Registers[_ipUserTuple] = 0;

            // Reset the program instruction counter.
            Registers[_pcSystemTuple] = 0;

            // Reset the flags register.
            Registers[Core.Register.Registers.FL] = 0;

            ResetStackPointer();

            SetHaltedState(false);
        }

        /// <summary>
        /// Sets the state of the flag to the specified state.
        /// </summary>
        /// <param name="aFlag">The flag to be set or cleared.</param>
        /// <param name="aState">
        /// The state to which the flag should be set.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetFlagState(CpuFlags aFlag, bool aState)
        {
            var flags =
                (CpuFlags)Registers[Core.Register.Registers.FL]
                & ~aFlag;

            if (aState)
            {
                flags |= aFlag;
            }

            Registers[Core.Register.Registers.FL] = (int)flags;
        }

        /// <summary>
        /// Clear or sets the result flag pair based on the 
        /// result of an operation.
        /// </summary>
        /// <param name="aResult">
        /// The result of the last operation performed.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetResultFlagPair(int aResult)
        {
            var flags = (CpuFlags)Registers[Core.Register.Registers.FL];
            flags &= ~CpuFlags.S & ~CpuFlags.Z;

            if (aResult < 0)
            {
                flags |= CpuFlags.S;
            }
            else if (aResult == 0)
            {
                flags |= CpuFlags.Z;
            }

            Registers[Core.Register.Registers.FL] = (int)flags;
        }

        /// <summary>
        /// Check if a given flag is set within the CPUs flag register.
        /// </summary>
        /// <param name="aFlag">
        /// The flag ID to be checked.
        /// </param>
        /// <returns>
        /// A boolean, true if the flag is set, false otherwise.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsFlagSet(CpuFlags aFlag)
        {
            return
                ((CpuFlags)Registers[Core.Register.Registers.FL])
                .HasFlag(aFlag);
        }

        /// <summary>
        /// Enable or disable logging within the CPU.
        /// </summary>
        /// <param name="aEnabled">
        /// The logging state of the CPU.
        /// </param>
        public void SetLoggingEnabled(bool aEnabled)
        {
            _isLoggingEnabled = aEnabled;
        }

        /// <summary>
        /// Set the address from which the execution of
        /// the binary should commence.
        /// </summary>
        /// <param name="aStartAddr"
        /// >The address from which the execution should commence.
        /// </param>
        public void SetStartAddress(int aStartAddr)
        {
            // Offset the starting address by the base
            // size of the memory. This is the area of memory
            // containing the system memory and stack.
            var offsetAddress = aStartAddr + Vm.Memory.BaseMemorySize;

            if (offsetAddress < 0 || offsetAddress >= Vm.Memory.Length)
            {
                throw new IndexOutOfRangeException
                (
                    "SetStartAddress: starting position is outside of " +
                    "the data bounds."
                );
            }

            // Set the instruction pointer register to the
            // specified starting position. Note that this does
            // -not- guarantee pointing at an instruction, so
            // setting this value incorrectly will lead
            // to the processor halting due to invalid data.
            // It is down to the user to ensure that this is
            // correct.
            // This will default to index zero if not set.
            Registers[_ipUserTuple] = offsetAddress;
        }

        /// <summary>
        /// Run the virtual machine with the current binary to completion.
        /// </summary>
        public void Run()
        {
            // Loop until we are instructed to halt.
            while (!IsHalted)
            {
                FetchExecuteNextInstruction();
            }
        }

        /// <summary>
        /// Step the virtual machine forward a single CPU cycle
        /// with the currently loaded binary.
        /// </summary>
        /// <returns>
        /// The data for the instruction that was executed.
        /// </returns>
        public InstructionData? Step()
        {
            return !IsHalted ? FetchExecuteNextInstruction() : null;
        }

        /// <summary>
        /// Fetch, decode and execute the next instruction.
        /// </summary>
        /// <returns>
        /// The data for the instruction that was executed.
        /// </returns>
        public InstructionData FetchExecuteNextInstruction()
        {
            var pos = Registers[_ipUserTuple];
            var opCodeStartPos = pos;

            OpCode opCode;
            try
            {
                opCode = Vm.Memory.GetOpCode(pos, UserCtx, true);
            }
            catch (Exception ex)
            {
                SetHaltedState(true);

                throw ex switch
                {
                    MemoryAccessViolationException _
                        => new MemoryAccessViolationException
                        (
                            "FetchExecuteNextInstruction: instruction " +
                            $"at position {opCodeStartPos} attempted " +
                            "to access memory with insufficient " +
                            $"permissions. {ex.Message}"
                        ),

                    // I do not know how this can happen, but just to be safe.
                    _
                        => new Exception
                        (
                            $"FetchExecuteNextInstruction: {ex.Message}"
                        ),
                };
            }

            if (!_instructionCache.TryGetValue(opCode,
                                               out var ins))
            {
                SetHaltedState(true);
                throw new InvalidDataException
                (
                    "FetchExecuteNextInstruction: Invalid opcode " +
                    $"ID '{opCode}' detected at position " +
                    $"{opCodeStartPos}."
                );
            }

            // Advance the instruction pointer by the number of bytes
            // corresponding to the size of the opcode (in bytes)
            Registers[_ipUserTuple] += sizeof(OpCode);
            pos += sizeof(OpCode);

            var asmIns = new InstructionData
            {
                OpCode = opCode
            };

            // The types of the arguments expected for this instruction.
            var argTypes = ins.ArgumentTypes;

            var iPos = pos;
            try
            {
                // Load the data representing the arguments.
                foreach (var t in argTypes)
                {
                    asmIns.Args.Add(new InstructionArg
                    {
                        Value = GetNextInstructionArgument(ref pos, t)
                    });
                }
            }
            catch (Exception ex)
            {
                SetHaltedState(true);

                throw ex switch
                {
                    EndOfStreamException _
                        => new EndOfStreamException
                        (
                            "FetchExecuteNextInstruction: expected " +
                            "number of arguments for opcode " +
                            $"'{asmIns.OpCode}' is " +
                            $"{argTypes.Length}, got {asmIns.Args.Count}."
                        ),

                    MemoryAccessViolationException _
                        => new MemoryAccessViolationException
                        (
                            "FetchExecuteNextInstruction: instruction " +
                            $"at position {opCodeStartPos} attempted " +
                            "to access memory with insufficient " +
                            $"permissions. {ex.Message}"
                        ),

                    // I do not know how this can happen, but just to be safe.
                    _
                        => new Exception
                        (
                            $"FetchExecuteNextInstruction: {ex.Message}"
                        ),
                };
            }

            // Advance the instruction pointer by the number of bytes
            // corresponding sum of the size of the arguments (in bytes).
            Registers[_ipUserTuple] += (pos - iPos);

            if (ExecuteInstruction(ins, asmIns))
            {
                SetHaltedState(true);
            }

            // Have we executed everything that needs to be executed?
            if (pos >= _maxExecutableBound)
            {
                SetHaltedState(true);
            }

            return asmIns;
        }

        /// <summary>
        /// Executes an opcode instruction against a given 
        /// instruction instance.
        /// </summary>
        /// <param name="aIns">
        /// The instruction instance against which the opcode 
        /// instruction should be executed.
        /// </param>
        /// <param name="aInsData">The opcode instruction.</param>
        /// <returns>
        /// A boolean indicating if the CPU should halt execution
        /// after completing this instruction.
        /// </returns>
        public bool ExecuteInstruction(Instruction aIns,
                                       InstructionData aInsData)
        {
            try
            {
                var ret = aIns.Execute(aInsData, this);

                // With each successful instruction execution,
                // increment the program counter.
                ++Registers[_pcSystemTuple];

                return ret;
            }
            catch (Exception ex)
            {
                SetHaltedState(true);

                var opCodeStartPos = Registers[_ipUserTuple];

                throw ex switch
                {
                    MemoryAccessViolationException _
                        => new MemoryAccessViolationException
                        (
                            "ExecuteInstruction: instruction at " +
                            $"position {opCodeStartPos} attempted " +
                            "to access memory with insufficient " +
                            $"permissions. {ex.Message}"
                        ),

                    MemoryOutOfRangeException _
                        => new MemoryOutOfRangeException
                        (
                            $"ExecuteInstruction: instruction at " +
                            $"position {opCodeStartPos} failed to " +
                            "access the specified memory location " +
                            "as it falls outside of the bounds of " +
                            "the memory region."
                        ),

                    RegisterAccessViolationException _
                        => new RegisterAccessViolationException
                        (
                            "ExecuteInstruction: instruction at " +
                            $"position {opCodeStartPos} encountered " +
                            "a permission error when trying to " +
                            $"operate on a register. {ex.Message}"
                        ),

                    KeyNotFoundException _
                        => new InvalidRegisterException
                        (
                            "ExecuteInstruction: instruction at " +
                            $"position {opCodeStartPos} failed " +
                            "to access the register specified: " +
                            "the specified register does not exist."
                        ),

                    DivideByZeroException _
                        => new DivideByZeroException
                        (
                            "ExecuteInstruction: instruction " +
                            $"at position {opCodeStartPos} " +
                            "triggered a division by zero " +
                            "exception."
                        ),

                    _
                        => new Exception
                        (
                            "ExecuteInstruction: failed to execute " +
                            "the CPU instruction at position " +
                            $"{opCodeStartPos}. {ex.Message}"
                        ),
                };
            }
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
            var pos = aStartAddr + Vm.Memory.BaseMemorySize;

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

            var disInstructions = new List<string>();

            while (pos >= minPos && pos < maxPos)
            {
                var s = 
                    aShowLocation ? $"{pos:X8} : " : string.Empty;

                s += DisassembleNextInstruction(ref pos);

                disInstructions.Add(s);
            }

            return disInstructions.ToArray();
        }

        /// <summary>
        /// Read an opcode instruction argument from memory.
        /// </summary>
        /// <param name="aPos">
        /// The position in memory from which to begin 
        /// reading the argument.
        /// </param>
        /// <param name="aT"
        /// >The type of the argument to be read.
        /// </param>
        /// <returns>
        /// An object containing the opcode instruction data.
        /// </returns>
        private object GetNextInstructionArgument(ref int aPos, Type aT)
        {
            object arg;
            switch (aT)
            {
                case { } when aT == typeof(byte):
                    arg = Vm.Memory.GetValue(aPos, UserCtx, true);
                    aPos += sizeof(byte);
                    break;

                case { } when aT == typeof(int):
                    arg = Vm.Memory.GetInt(aPos, UserCtx, true);
                    aPos += sizeof(int);
                    break;

                case { } when aT == typeof(string):
                    // Strings are special as their size
                    // cannot be determined directly from
                    // their type.
                    // So here we need to add the number of bytes
                    // we read to the position marker.
                    var (bLen, s) =
                        Vm.Memory.GetString(aPos, UserCtx, true);
                    arg = s;
                    aPos += bLen;
                    break;

                case { } when aT == typeof(Registers):
                    arg = 
                        Vm.Memory.GetRegisterIdent(aPos, UserCtx, true);
                    aPos += sizeof(byte);
                    break;

                default:
                    throw new NotSupportedException
                    (
                        $"GetNextInstructionArgument: the type {aT} was " +
                        "passed as an argument type, but no support " +
                        "has been provided for that type."
                    );
            }

            return arg;
        }

        /// <summary>
        /// Sets any breakpoint trigger hooks that have been specified.
        /// </summary>
        private void SetBreakpointObservers()
        {
            if (Vm.Debugger.Breakpoints.Count == 0)
            {
                return;
            }

            void InstructionPointerBp(int aPos)
            {
                var halt = Vm.Debugger
                    .TriggerBreakpoint(aPos, Breakpoint.BreakpointType.PC);

                SetHaltedState(halt);
            }

            void ProgramCounterBp(int aPos)
            {
                var halt = Vm.Debugger
                    .TriggerBreakpoint(aPos, Breakpoint.BreakpointType.PC);

                SetHaltedState(halt);
            }

            // If we have any instruction pointer breakpoints
            // then we need to add the handler for those now.
            if (Vm.Debugger
                .HasBreakpointOfType(Breakpoint.BreakpointType.IP))
            {
                _hasIpBreakpoint = true;
                Registers.Hook(Core.Register.Registers.IP,
                               InstructionPointerBp,
                               Register.HookTypes.Change);
            }

            // If we have any program counter breakpoints
            // then we need to add the handler for those now.
            if (!Vm.Debugger
                .HasBreakpointOfType(Breakpoint.BreakpointType.PC))
            {
                return;
            }

            _hasPcBreakpoint = true;
            Registers.Hook(Core.Register.Registers.PC,
                           ProgramCounterBp,
                           Register.HookTypes.Change);
        }

        /// <summary>
        /// Clear any breakpoint trigger hooks that have been set.
        /// </summary>
        private void ClearBreakpoints()
        {
            Vm.Debugger.RemoveAllBreakpoints();
            _hasIpBreakpoint = false;
            _hasPcBreakpoint = false;
        }

        /// <summary>
        /// Used to disassemble the next instruction.
        /// Essentially a clone of FetchExecuteNextInstruction
        /// but without the exception throwing code.
        /// </summary>
        /// <param name="aPos">
        /// The position in memory from which to begin 
        /// reading the instruction.
        /// </param>
        /// <returns>
        /// A string giving the disassembly of the next instruction.
        /// </returns>
        private string DisassembleNextInstruction(ref int aPos)
        {
            OpCode op;
            try
            {
                op = 
                    Vm.Memory.GetOpCode(aPos, SysCtx, true);
            }
            catch
            {
                return string.Empty;
            }

            if (!Enum.IsDefined(typeof(OpCode), op))
            {
                // We do not recognize this opcode and so
                // we would have no meaningful output
                // here at all. Return the byte code instead.
                return $"???? {op:X2}";
            }

            // No instruction matching the OpCode was found.
            // In practice this shouldn't happen, except in a malformed
            // binary file.
            if (!_instructionCache.TryGetValue(op,
                                               out var ins))
            {
                // Return the byte code as that's all we can
                // safely provide.
                return $"???? {op:X2}";
            }

            aPos += sizeof(OpCode);

            var opIns = new InstructionData
            {
                OpCode = op
            };

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
                        Value = GetNextInstructionArgument(ref aPos, t)
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

        /// <summary>
        /// Enable or disable the halted state of the CPU.
        /// </summary>
        /// <param name="aState">
        /// The halt state to apply to the CPU.
        /// </param>
        private void SetHaltedState(bool aState)
        {
            IsHalted = aState;
        }

        /// <summary>
        /// Reset the stack pointer to the bottom of the stack
        /// memory region.
        /// </summary>
        private void ResetStackPointer()
        {
            // Reset the stack pointer to the bottom of the
            // stack memory region.
            Registers[Core.Register.Registers.SP] =
                Vm.Memory.StackEnd;
        }
    }
}
