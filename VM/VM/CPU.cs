using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using VMCore.VM.Core;
using VMCore.VM.Core.Exceptions;
using VMCore.VM.Core.Reg;

namespace VMCore.VM
{
    public class CPU
    {
        /// <summary>
        /// The memory region sequence ID that this CPU
        /// instance was assigned to begin executing.
        /// </summary>
        public int MemExecutableSeqID { get; private set; }

        /// <summary>
        /// A boolean indicating if the CPU is currently halted.
        /// </summary>
        public bool IsHalted { get; set; } = false;

        /// <summary>
        /// The list of registers associated with this CPU instance.
        /// </summary>
        public RegisterCollection Registers { get; set; }

        /// <summary>
        /// The VM instance that holds this CPU.
        /// </summary>
        public VirtualMachine VM { get; private set; }

        /// <summary>
        /// A cached of the opcodes to their instruction instances.
        /// </summary>
        /// <remarks>
        /// Since the CPU cannot be run on its own then this is safe
        /// to use here as the virtual machine parent will always
        /// have called the method to build these caches.
        /// </remarks>
        private Dictionary<OpCode, Instruction> _instructionCache = 
            ReflectionUtils.InstructionCache;

#if DEBUG
        private bool _isLoggingEnabled { get; set; } = true;
#else
        private bool _isLoggingEnabled { get; set; } = false;
#endif

        /// <summary>
        /// An internal binary reader, for populating the bytecode data above.
        /// </summary>
        //private BinaryReader _br;

        /// <summary>
        /// An internal indicator of if an IP breakpoint has been triggered.
        /// </summary>
        private bool _hasIPBreakpoint = false;

        /// <summary>
        /// An internal indicator of if a PC breakpoint has been triggered.
        /// </summary>
        private bool _hasPCBreakpoint = false;

        /// <summary>
        /// A dictionary mapping the CPU flags to their respective position
        /// within the enum. Used for bitshifting.
        /// </summary>
        private Dictionary<CPUFlags, int> _flagIndicies
            = new Dictionary<CPUFlags, int>();

        private const SecurityContext _userCtx = 
            SecurityContext.User;

        private const SecurityContext _sysCtx =
            SecurityContext.System;

        /// <summary>
        /// A IP/user register tuple to avoid having to repeatedly create one.
        /// </summary>
        private readonly (Registers, SecurityContext) _ipUserTuple
            = (VMCore.Registers.IP, _userCtx);

        /// <summary>
        /// A PC/system register tuple to avoid having to repeatedly create one.
        /// </summary>
        private readonly (Registers, SecurityContext) _pcSystemTuple
            = (VMCore.Registers.PC, _sysCtx);

        /// <summary>
        /// The lower memory bound from which data can be read or written
        /// within the program.
        /// </summary>
        private int _minExecutableBound;

        /// <summary>
        /// The upper memory bound from which data can be read or written
        /// within the program.
        /// </summary>
        private int _maxExecutableBound;

        /// <summary>
        /// If this CPU can swap between executable memory regions.
        /// </summary>
        private bool _canSwapMemoryRegions;

        /// <summary>
        /// Create a new CPU instance.
        /// </summary>
        /// <param name="aVm">The virtual machine instance to which this CPU belongs.</param>
        /// <param name="aCanSwapMemoryRegions">A boolean, true if the CPU will be permitted to swap between executable memory regions, false otherwise.</param>
        public CPU(VirtualMachine aVm,
                   bool aCanSwapMemoryRegions = false)
        {
            VM = aVm;
            Registers = new RegisterCollection(this);

            var flags = (CPUFlags[])Enum.GetValues(typeof(CPUFlags));
            for (var i = 0; i < flags.Length; i++)
            {
                _flagIndicies.Add(flags[i], i);
            }

            _canSwapMemoryRegions = aCanSwapMemoryRegions;
        }

        ~CPU()
        {
        }

        /// <summary>
        /// Sets the state of the flag to the specified state.
        /// </summary>
        /// <param name="aFlag">The flag to be set or cleared.</param>
        /// <param name="aState">The state to which the flag should be set.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetFlagState(CPUFlags aFlag, bool aState)
        {
            Registers[VMCore.Registers.FL] = 
                Utils.SetBitState(Registers[VMCore.Registers.FL],
                                  _flagIndicies[aFlag],
                                  aState ? 1 : 0);
        }

        /// <summary>
        /// Clear or sets the result flag pair based on the result of an operation.
        /// </summary>
        /// <param name="aResult">The result of the last operation performed.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetResultFlagPair(int aResult)
        {
            // I have intentionally chosen not to use the flags
            // feature of enums as it is slower than bit shifting.
            // See:
            // https://stackoverflow.com/questions/7368652/what-is-it-that-makes-enum-hasflag-so-slow
            // If this ever improved then this can be rewritten
            // to make use of it.

            var maskSign = 1 << _flagIndicies[CPUFlags.S];
            var maskZero = 1 << _flagIndicies[CPUFlags.Z];

            // Clear the signed and zero flags
            // as these will always need to be
            // cleared.
            var flags = Registers[VMCore.Registers.FL] 
                & ~maskSign & ~maskZero;

            if (aResult < 0)
            {
                flags |= (1 << _flagIndicies[CPUFlags.S]) & maskSign;
            }
            else if (aResult == 0)
            {
                flags |= (1 << _flagIndicies[CPUFlags.Z]) & maskZero;
            }

            Registers[VMCore.Registers.FL] = flags;
        }

        /// <summary>
        /// Check if a given flag is set within the CPUs flag register.
        /// </summary>
        /// <param name="aFlag">The flag ID to be checked.</param>
        /// <returns>A boolean, true if the flag is set, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsFlagSet(CPUFlags aFlag)
        {
            return 
                Utils.IsBitSet(Registers[VMCore.Registers.FL],
                               _flagIndicies[aFlag]);
        }

        /// <summary>
        /// Execute a reset on the CPU, clearing any binary data and resetting
        /// various registers within the CPU.
        /// </summary>
        public void Reset()
        {
            // Reset the instruction pointer.
            Registers[_ipUserTuple] = 0;

            // Reset the program instruction counter.
            Registers[_pcSystemTuple] = 0;

            // Reset the flags register.
            Registers[(VMCore.Registers.FL, _sysCtx)] = 0;

            // TODO - reset stack pointer here.

            SetHaultedState(false);
        }

        /// <summary>
        /// Enable or disable logging within the CPU.
        /// </summary>
        /// <param name="aEnabled">The logging state of the CPU.</param>
        public void SetLoggingEnabled(bool aEnabled)
        {
            _isLoggingEnabled = aEnabled;
        }

        /// <summary>
        /// Set the address from which the execution of the binary should commence.
        /// </summary>
        /// <param name="aStartAddr">The address from which the execution should commence.</param>
        public void SetStartAddress(int aStartAddr)
        {
            // Offset the starting address by the base
            // size of the memory. This is the area of memory
            // containing the system memory and stack.
            int offsetAddress = aStartAddr + VM.Memory.BaseMemorySize;

            //if (aStartAddress < 0 || aStartAddress >= _data.Length)
            if (offsetAddress < 0 || offsetAddress >= VM.Memory.Length)
            {
                throw new IndexOutOfRangeException("SetStartAddress: starting position is outside of the data bounds.");
            }

            // Set the instruction pointer register to the
            // specified starting position. Note that this does
            // -not- guarantee pointing at an instruction, so
            // setting this value incorrectly will lead
            // to the processor halting due to invalid data.
            // It is down to the user to ensure that this is
            // correct.
            // This will default to index zero if not set.
            Registers[_ipUserTuple] =
                offsetAddress;
        }

        /// <summary>
        /// Run the virtual machine with the current binary to completion.
        /// </summary>
        /// <param name="aMemSeqID">The sequence ID for the memory region containing the code.</param>
        /// <param name="aStartAddr">The address from which the execution should commence.</param>
        public void Run(int aMemSeqID, int aStartAddr = 0)
        {
            MemExecutableSeqID = aMemSeqID;

            if (_canSwapMemoryRegions)
            {
                _minExecutableBound = 0;
                _maxExecutableBound = VM.Memory.Length;
            }
            else
            {
                var region =
                    VM.Memory.GetMemoryRegion(aMemSeqID);
                _minExecutableBound = region.Start;
                _maxExecutableBound = region.End;
            }

            SetStartAddress(aStartAddr);

            SetBreakpointObservers();

            // Loop until we are instructed to halt.
            while (!IsHalted)
            {
                FetchExecuteNextInstruction();
            }
        }

        /// <summary>
        /// Step the virtual machine forward a single CPU cycle with the current binary.
        /// </summary>
        /// <param name="aMemSeqID">The sequence ID for the memory region containing the code.</param>
        /// <param name="aStartAddr">The address from which the execution should commence.</param>
        public void Step(int aMemSeqID, int aStartAddr = 0)
        {
            MemExecutableSeqID = aMemSeqID;

            if (_canSwapMemoryRegions)
            {
                _minExecutableBound = 0;
                _maxExecutableBound = VM.Memory.Length;
            }
            else
            {
                var region =
                    VM.Memory.GetMemoryRegion(aMemSeqID);
                _minExecutableBound = region.Start;
                _maxExecutableBound = region.End;
            }

            SetStartAddress(aStartAddr);

            SetBreakpointObservers();

            if (!IsHalted)
            {
                FetchExecuteNextInstruction();
            }
        }

        /// <summary>
        /// Fetch, decode and execute the next instruction.
        /// </summary>
        public void FetchExecuteNextInstruction()
        {
            var pos = Registers[_ipUserTuple];
            if (pos < _minExecutableBound ||
                pos > _maxExecutableBound)
            {
                SetHaultedState(true);
                return;
            }

            var opCodeStartPos = pos;

            OpCode opCode = 
                    VM.Memory.GetOpCode(pos, _userCtx, true);

            if (!_instructionCache.TryGetValue(opCode,
                                               out Instruction ins))
            {
                SetHaultedState(true);
                throw new InvalidDataException($"FetchExecuteNextInstruction: Invalid opcode ID '{opCode}' detected at position {opCodeStartPos}.");
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
                SetHaultedState(true);

                throw ex switch
                {
                    // There was not enough data to provide the required number of arguments.
                    EndOfStreamException _  
                        => new EndOfStreamException($"FetchExecuteNextInstruction: Expected number of arguments for opcode '{asmIns.OpCode}' is {argTypes.Length}, got {asmIns.Args.Count}."),

                    // I do not know how this can happen, but just to be safe.
                    _                       
                        => new Exception($"FetchExecuteNextInstruction: {ex.Message}"),
                };
            }

            // Advance the instruction pointer by the number of bytes
            // corresponding sum of the size of the arguments (in bytes).
            Registers[_ipUserTuple] += (pos - iPos);

            if (ExecuteInstruction(ins, asmIns))
            {
                SetHaultedState(true);
            }
        }

        /// <summary>
        /// Executes an opcode instruction against a given instruction instance.
        /// </summary>
        /// <param name="aIns">The instruction instance against which the opcode instruction should be executed.</param>
        /// <param name="aInsData">The opcode instruction.</param>
        /// <returns>A boolean indicating if the CPU should halt execution after completing this instruction.</returns>
        public bool ExecuteInstruction(Instruction aIns,
                                       InstructionData aInsData)
        {
            try
            {
                var ret = aIns.Execute(aInsData, this);

                // With each successful instruction execution, increment the program counter.
                ++Registers[_pcSystemTuple];

                return ret;
            }
            catch (Exception ex)
            {
                SetHaultedState(true);

                var opCodeStartPos = Registers[_ipUserTuple];

                throw ex switch
                {
                    MemoryAccessViolationException _
                        => new AccessViolationException($"ExecuteInstruction: instruction at position {opCodeStartPos} attempted to access memory with insufficient permissions. {ex.Message}"),

                    MemoryOutOfRangeException _
                        => new MemoryOutOfRangeException($"ExecuteInstruction: instruction at position {opCodeStartPos} failed to access the specified memory location as it falls outside of the bounds of the memory region."),
                    
                    RegisterAccessViolationException _ 
                        => new RegisterAccessViolationException($"ExecuteInstruction: instruction at position {opCodeStartPos} encountered a permission error when trying to operate on a register. {ex.Message}"),
                    
                    KeyNotFoundException _
                        => new InvalidRegisterException($"ExecuteInstruction: instruction at position {opCodeStartPos} failed to access the register specified: the specified register does not exist."),
                    
                    DivideByZeroException _
                        => new DivideByZeroException($"ExecuteInstruction: instruction at position {opCodeStartPos} triggered a division by zero exception."),
                    
                    _
                        => new Exception($"ExecuteInstruction: failed to execute the CPU instruction at position {opCodeStartPos}. {ex.Message} {aInsData}"),
                };
            }
        }

        /// <summary>
        /// Converts the byte code of a program back into assembly.
        /// This will skip any exceptions that would otherwise be thrown
        /// when executing this code.
        /// </summary>
        /// <param name="aMemSeqID">The sequence ID for the memory region containing the code.</param>
        /// <param name="aShowLocation">If the binary locations of the commands should be shown.</param>
        /// <param name="aStartAddr">The address from which the execution should commence.</param>
        /// <returns>A string array containing one instruction per entry.</returns>
        public string[] Disassemble(int aMemSeqID,
                                    bool aShowLocation = false,
                                    int aStartAddr = 0)
        {
            // Reset the position of the stream back to
            // the start.
            int pos = aStartAddr + VM.Memory.BaseMemorySize;

            var region =
                VM.Memory.GetMemoryRegion(aMemSeqID);
            var minPos = region.Start;
            var maxPos = region.End;

            List<string> disInstructions = new List<string>();

            string s;
            while (pos >= minPos && pos <= maxPos)
            {
                if (aShowLocation)
                {
                    s = $"{pos:X8} : ";
                }
                else
                {
                    s = string.Empty;
                }

                s += DisassembleNextInstruction(ref pos);

                disInstructions.Add(s);
            }

            return disInstructions.ToArray();
        }

        /// <summary>
        /// Read an opcode instruction argument from memory.
        /// </summary>
        /// <param name="pos">The position in memory from which to begin reading the argument.</param>
        /// <param name="t">The type of the argument to be read.</param>
        /// <returns>An object containing the opcode instruction data.</returns>
        private object GetNextInstructionArgument(ref int pos, Type t)
        {
            object arg;
            switch (t)
            {
                case Type _ when t == typeof(byte):
                    arg = VM.Memory.GetValue(pos, _userCtx, true);
                    pos += sizeof(byte);
                    break;

                case Type _ when t == typeof(int):
                    arg = VM.Memory.GetInt(pos, _userCtx, true);
                    pos += sizeof(int);
                    break;

                case Type _ when t == typeof(string):
                    // Strings are special as their size
                    // cannot be determined directly from
                    // their type.
                    // So here we need to add the number of bytes
                    // we read to the position marker.
                    (int bLen, string s) =
                        VM.Memory.GetString(pos, _userCtx, true);
                    arg = s;
                    pos += bLen;
                    break;

                case Type _ when t == typeof(Registers):
                    arg = VM.Memory.GetRegister(pos, _userCtx, true);
                    pos += sizeof(byte);
                    break;

                default:
                    throw new NotSupportedException($"GetNextInstructionArgument: the type {t} was passed as an argument type, but no support has been provided for that type.");
                    break;
            }

            return arg;
        }

        /// <summary>
        /// Sets any breakpoint trigger hooks that have been specified.
        /// </summary>
        private void SetBreakpointObservers()
        {
            if (VM.Debugger.Breakpoints.Count == 0)
            {
                return;
            }

            void instructionPointerBP(int pos)
            {
                var halt = VM.Debugger
                    .TriggerBreakpoint(pos, Breakpoint.BreakpointType.PC);

                SetHaultedState(halt);
            }

            void programCounterBP(int pos)
            {
                var halt = VM.Debugger
                    .TriggerBreakpoint(pos, Breakpoint.BreakpointType.PC);

                SetHaultedState(halt);
            }

            // If we have any instruction pointer breakpoints
            // then we need to add the handler for those now.
            if (VM.Debugger
                .HasBreakpointOfType(Breakpoint.BreakpointType.IP))
            {
                _hasIPBreakpoint = true;
                Registers.Hook(VMCore.Registers.IP,
                               instructionPointerBP,
                               Register.HookTypes.Change);
            }

            // If we have any program counter breakpoints
            // then we need to add the handler for those now.
            if (VM.Debugger
                .HasBreakpointOfType(Breakpoint.BreakpointType.PC))
            {
                _hasPCBreakpoint = true;
                Registers.Hook(VMCore.Registers.PC,
                               programCounterBP,
                               Register.HookTypes.Change);
            }
        }

        /// <summary>
        /// Clear any breakpoint trigger hooks that have been set.
        /// </summary>
        private void ClearBreakpoints()
        {
            VM.Debugger.RemoveAllBreakpoints();
            _hasIPBreakpoint = false;
            _hasPCBreakpoint = false;
        }

        /// <summary>
        /// Used to disassemble the next instruction.
        /// Essentially a clone of FetchExecuteNextInstruction
        /// but without the exception throwing code.
        /// </summary>
        // <param name="pos">The position in memory from which to begin reading the instruction.</param>
        /// <returns>A string giving the disassembly of the next instruction.</returns>
        private string DisassembleNextInstruction(ref int pos)
        {
            OpCode opCode =
                    VM.Memory.GetOpCode(pos, _userCtx, true);

            if (!Enum.IsDefined(typeof(OpCode), opCode))
            {
                // We do not recognize this opcode and so
                // we would have no meaningful output
                // here at all. Return the byte code instead.
                return $"???? {opCode:X2}";
            }

            // No instruction matching the OpCode was found.
            // In practice this shouldn't happen, except in a malformed
            // binary file.
            if (!_instructionCache.TryGetValue(opCode,
                                               out Instruction ins))
            {
                // Return the byte code as that's all we can
                // safely provide.
                return $"???? {opCode:X2}";
            }

            pos += sizeof(OpCode);

            var opIns = new InstructionData
            {
                OpCode = opCode
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
                        Value = GetNextInstructionArgument(ref pos, t)
                    });
                }
            }
            catch { }

            // If the number of arguments is less than expected then the
            // data is malformed.
            // Try to get the most information that we can
            // from this but the data is likely to be useless.
            if (opIns.Args.Count < ins.ArgumentTypes.Length)
            {
                string s = $"{ins.AsmName}";
                foreach (var arg in opIns.Args)
                {
                    s += $" {arg:X2}";
                }

                return s;
            }
            else
            {
                return ins.ToString(opIns);
            }
        }

        /// <summary>
        /// Enable or disable the halted state of the CPU.
        /// </summary>
        /// <param name="aState">The halt state to apply to the CPU.</param>
        private void SetHaultedState(bool aState)
        {
            IsHalted = aState;
        }
    }
}
