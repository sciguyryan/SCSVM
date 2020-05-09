using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using VMCore.Expressions;
using VMCore.VM.Core;
using VMCore.VM.Core.Exceptions;
using VMCore.VM.Core.Reg;

namespace VMCore.VM
{
    public class CPU
    {
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
        /// An internal array of the bytecode data.
        /// </summary>
        private byte[] _data;

        /// <summary>
        /// An internal binary reader, for populating the bytecode data above.
        /// </summary>
        private BinaryReader _br;

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

        public CPU(VirtualMachine vm)
        {
            VM = vm;
            Registers = new RegisterCollection(this);

            var flags = (CPUFlags[])Enum.GetValues(typeof(CPUFlags));
            for (var i = 0; i < flags.Length; i++)
            {
                _flagIndicies.Add(flags[i], i);
            }
        }

        ~CPU()
        {
            ClearData();
        }

        /// <summary>
        /// Sets the state of the flag to the specified state.
        /// </summary>
        /// <param name="flag">The flag to be set or cleared.</param>
        /// <param name="state">The state to which the flag should be set.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetFlagState(CPUFlags flag, bool state)
        {
            Registers[VMCore.Registers.FL] = 
                Utils.SetBitState(Registers[VMCore.Registers.FL],
                                  _flagIndicies[flag],
                                  state ? 1 : 0);
        }

        /// <summary>
        /// Clear or sets the result flag pair based on the result of an operation.
        /// </summary>
        /// <param name="result">The result of the last operation performed.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetResultFlagPair(int result)
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

            if (result < 0)
            {
                flags |= (1 << _flagIndicies[CPUFlags.S]) & maskSign;
            }
            else if (result == 0)
            {
                flags |= (1 << _flagIndicies[CPUFlags.Z]) & maskZero;
            }

            Registers[VMCore.Registers.FL] = flags;
        }

        /// <summary>
        /// Check if a given flag is set within the CPUs flag register.
        /// </summary>
        /// <param name="flag">The flag ID to be checked.</param>
        /// <returns>A boolean, true if the flag is set, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsFlagSet(CPUFlags flag)
        {
            return 
                Utils.IsBitSet(Registers[VMCore.Registers.FL],
                               _flagIndicies[flag]);
        }

        /// <summary>
        /// Load a binary containing byte code instructions into the CPU.
        /// </summary>
        /// <param name="data">A byte array containing the binary byte code instructions.</param>
        /// <param name="startAddress">The address from which the execution should commence.</param>
        public void LoadData(byte[] data, int startAddress = 0)
        {
            _data = data;
            SetStartAddress(startAddress);

            _br = new BinaryReader(new MemoryStream(data));
        }

        /// <summary>
        ///  Clear the binary data from the CPU and close any associated handles.
        /// </summary>
        public void ClearData()
        {
            _data = null;
            _br = null;
        }

        /// <summary>
        /// Execute a reset on the CPU, clearing any binary data and resetting
        /// various registers within the CPU.
        /// </summary>
        public void Reset()
        {
            // Clear the opcode data.
            ClearData();

            // Reset the instruction pointer.
            Registers[(VMCore.Registers.IP, SecurityContext.System)] = 0;

            // Clear the flags register.
            Registers[(VMCore.Registers.FL, SecurityContext.System)] = 0;

            // Reset the program instruction counter.
            Registers[(VMCore.Registers.PC, SecurityContext.System)] = 0;

            // Reset the flags register.
            Registers[(VMCore.Registers.FL, SecurityContext.System)] = 0;

            // TODO - reset stack pointer here.

            SetHaultedState(false);
        }

        /// <summary>
        /// Enable or disable logging within the CPU.
        /// </summary>
        /// <param name="enabled">The logging state of the CPU.</param>
        public void SetLoggingEnabled(bool enabled)
        {
            _isLoggingEnabled = enabled;
        }

        /// <summary>
        /// Set the address from which the execution of the binary should commence.
        /// </summary>
        /// <param name="startAddress">The address from which the execution should commence.</param>
        public void SetStartAddress(int startAddress)
        {
            if (startAddress < 0 || startAddress >= _data.Length)
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
            Registers[(VMCore.Registers.IP, SecurityContext.System)] = 
                startAddress;
        }

        /// <summary>
        /// Run the virtual machine with the current binary to completion.
        /// </summary>
        public void Run()
        {
            // TODO - create a custom file format?

            if (_data.Length == 0)
            {
                // TODO - do something a bit better here.
                return;
            }

            SetBreakpointObservers();

            // Start reading instructions at the specified
            // starting position. This will usually be 0.
            _br.BaseStream.Position = 
                Registers[(VMCore.Registers.IP, SecurityContext.System)];

            // Loop until we are instructed to halt or until
            // the instruction pointer equals the length of the data.
            while (!IsHalted && 
                   Registers[(VMCore.Registers.IP, SecurityContext.System)] < _data.Length)
            {
                FetchExecuteNextInstruction();
            }
        }

        /// <summary>
        /// Step the virtual machine forward a single CPU cycle with the current binary.
        /// </summary>
        public void Step()
        {
            // TODO - create a custom file format?

            if (_data.Length == 0)
            {
                // TODO - do something a bit better here.
                return;
            }

            SetBreakpointObservers();

            // Start reading instructions at the specified
            // starting position. This will usually be 0.
            _br.BaseStream.Position = 
                Registers[(VMCore.Registers.IP, SecurityContext.System)];

            if (!IsHalted && 
                Registers[(VMCore.Registers.IP, SecurityContext.System)] < _data.Length)
            {
                FetchExecuteNextInstruction();
            }
        }

        /// <summary>
        /// Fetch, decode and execute the next instruction.
        /// </summary>
        public void FetchExecuteNextInstruction()
        {
            var opCodeStartPos = _br.BaseStream.Position;
            var opCode = _br.ReadInt32();

            if (!_instructionCache.TryGetValue((OpCode)opCode, out Instruction ins))
            {
                SetHaultedState(true);
                throw new InvalidDataException($"FetchExecuteNextInstruction: Invalid opcode ID '{opCode}' detected at position {opCodeStartPos}.");
            }

            // Advance the instruction pointer by the number of bytes
            // corresponding to the size of the opcode (in bytes)
            Registers[(VMCore.Registers.IP, SecurityContext.System)] += 
                sizeof(OpCode);

            var asmIns = new InstructionData
            {
                OpCode = (OpCode)opCode
            };

            // The types of the arguments expected for this instruction.
            var argTypes = ins.ArgumentTypes;

            // This will give the size of the basic types.
            // This does not include the length of a string
            // which cannot be calculated until we have
            // parsed it.
            var argSize = ins.ArgumentByteSize;

            try
            {
                // Load the data representing the arguments.
                foreach (var t in argTypes)
                {
                    var p = _br.BaseStream.Position;

                    asmIns.Args.Add(new AsmInstructionArg
                    {
                        Value = Utils.ReadDataByType(t, _br)
                    });

                    // We need to do something a bit more
                    // interesting here.
                    // We need to add the length of the
                    // string plus one byte to the
                    // argument size to account for the
                    // extra read data.
                    if (t == typeof(string))
                    {
                        argSize += (int)(_br.BaseStream.Position - p);
                    }
                }
            }
            catch (Exception ex)
            {
                SetHaultedState(true);

                throw ex switch
                {
                    // There was not enough data to provide the required number of arguments.
                    EndOfStreamException _  => new EndOfStreamException($"FetchExecuteNextInstruction: Expected number of arguments for opcode '{asmIns.OpCode}' is {argTypes.Length}, got {asmIns.Args.Count}."),
                    // I do not know how this can happen, but just to be safe.
                    _                       => new Exception($"FetchExecuteNextInstruction: {ex.Message}"),
                };
            }

            // Advance the instruction pointer by the number of bytes
            // corresponding sum of the size of the arguments (in bytes).
            Registers[(VMCore.Registers.IP, SecurityContext.System)] +=
                argSize;

            if (ExecuteInstruction(ins, asmIns))
            {
                SetHaultedState(true);
            }
        }


        /// <summary>
        /// Executes an opcode instruction against a given instruction instance.
        /// </summary>
        /// <param name="ins">The instruction instance against which the opcode instruction should be executed.</param>
        /// <param name="asmIns">The opcode instruction.</param>
        /// <returns>A boolean, true indicating that the CPU should halt execution and false otherwise.</returns>
        public bool ExecuteInstruction(Instruction ins, InstructionData asmIns)
        {
            try
            {
                var ret = ins.Execute(asmIns, this);

                // With each successful instruction execution, increment the program counter.
                ++Registers[(VMCore.Registers.PC, SecurityContext.System)];

                return ret;
            }
            catch (Exception ex)
            {
                SetHaultedState(true);

                var opCodeStartPos = 
                    Registers[(VMCore.Registers.IP, SecurityContext.System)];

                throw ex switch
                {
                    MemoryAccessViolationException _    => new AccessViolationException($"ExecuteInstruction: instruction at position {opCodeStartPos} attempted to access memory with insufficient permissions. {ex.Message}"),
                    MemoryOutOfRangeException _         => new MemoryOutOfRangeException($"ExecuteInstruction: instruction at position {opCodeStartPos} failed to access the specified memory location as it falls outside of the bounds of the memory region."),
                    RegisterAccessViolationException _  => new RegisterAccessViolationException($"ExecuteInstruction: instruction at position {opCodeStartPos} encountered a permission error when trying to operate on a register. {ex.Message}"),
                    KeyNotFoundException _              => new InvalidRegisterException($"ExecuteInstruction: instruction at position {opCodeStartPos} failed to access the register specified: the specified register does not exist."),
                    DivideByZeroException _             => new DivideByZeroException($"ExecuteInstruction: instruction at position {opCodeStartPos} triggered a division by zero exception."),
                    _                                   => new Exception($"ExecuteInstruction: failed to execute the CPU instruction at position {opCodeStartPos}. {ex.Message} {asmIns}"),
                };
            }
        }

        /// <summary>
        /// Converts the byte code of a program back into assembly.
        /// This will skip any exceptions that would otherwise be thrown
        /// when executing this code.
        /// </summary>
        /// <param name="showLocation">If the binary locations of the commands should be shown.</param>
        /// <returns>A string array containing one instruction per entry.</returns>
        public string[] Disassemble(bool showLocation = false, int startAddress = 0)
        { 
            // Reset the position of the stream back to
            // the start.
            _br.BaseStream.Position = startAddress;

            List<string> disInstructions = new List<string>();

            string s;
            while (_br.BaseStream.Position < _data.Length)
            {
                if (showLocation)
                {
                    s = $"{_br.BaseStream.Position:X8} : ";
                }
                else
                {
                    s = string.Empty;
                }

                s += DisassembleNextInstruction();

                disInstructions.Add(s);
            }

            return disInstructions.ToArray();
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
                var hault = VM.Debugger
                    .TriggerBreakPoint(pos, Breakpoint.BreakpointType.PC);

                SetHaultedState(hault);
            }

            void programCounterBP(int pos)
            {
                var hault = VM.Debugger
                    .TriggerBreakPoint(pos, Breakpoint.BreakpointType.PC);

                SetHaultedState(hault);
            }

            // If we have any instruction pointer breakpoints
            // then we need to add the handler for those now.
            if (VM.Debugger.HasBreakPointOfType(Breakpoint.BreakpointType.IP))
            {
                _hasIPBreakpoint = true;
                Registers.Hook(VMCore.Registers.IP,
                               instructionPointerBP,
                               Register.HookTypes.Change);
            }

            // If we have any program counter breakpoints
            // then we need to add the handler for those now.
            if (VM.Debugger.HasBreakPointOfType(Breakpoint.BreakpointType.PC))
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
        /// Used to disassemble the next instruction. Essentially a clone of
        /// FetchExecuteNextInstruction but without the exception code.
        /// </summary>
        /// <returns>A string giving the disassembly of the next instruction.</returns>
        private string DisassembleNextInstruction()
        {
            var opCode = _br.ReadInt32();

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
            if (!_instructionCache.TryGetValue((OpCode)opCode, out Instruction ins))
            {
                // Return the byte code as that's all we can
                // safely provide.
                return $"???? {opCode:X2}";
            }

            var opIns = new InstructionData
            {
                OpCode = (OpCode)opCode
            };

            // The types of the arguments expected for this instruction.
            var argTypes = ins.ArgumentTypes;

            // Iterate through the list of arguments and attempt
            // to populate the data.
            try
            {
                foreach (var t in argTypes)
                {
                    var arg = new AsmInstructionArg
                    {
                        Value = Utils.ReadDataByType(t, _br)
                    };

                    opIns.Args.Add(arg);
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
        /// <param name="state">The halt state to apply to the CPU.</param>
        private void SetHaultedState(bool state)
        {
            IsHalted = state;
        }
    }
}
