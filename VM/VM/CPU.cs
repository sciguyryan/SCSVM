﻿#nullable enable

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
        private readonly Dictionary<OpCode, Instruction> _instructionCache =
            ReflectionUtils.InstructionCache;

#if DEBUG
        private bool _isLoggingEnabled = true;
#else
        private bool _isLoggingEnabled = false;
#endif

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
        /// A IP/user tuple to avoid having to repeatedly create one.
        /// </summary>
        private readonly (Registers, SecurityContext) _ipUserTuple
            = (Core.Register.Registers.IP, UserCtx);

        /// <summary>
        /// A PC/system tuple to avoid having to repeatedly create one.
        /// </summary>
        private readonly (Registers, SecurityContext) _pcSystemTuple
            = (Core.Register.Registers.PC, SysCtx);

        /// <summary>
        /// A FP/system tuple to avoid having to repeatedly create one.
        /// </summary>
        private readonly (Registers, SecurityContext) _fpSystemTuple
            = (Core.Register.Registers.FP, SysCtx);

        /// <summary>
        /// A SP/system tuple to avoid having to repeatedly create one.
        /// </summary>
        private readonly (Registers, SecurityContext) _spSystemTuple
            = (Core.Register.Registers.SP, SysCtx);

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

        /// <summary>
        /// The size of the current stack frame.
        /// </summary>
        public int StackFrameSize;

        /// <summary>
        /// A list of registers to be saved to the stack when
        /// entering a subroutine.
        /// </summary>
        private readonly Registers[] _stateRegistersSave = 
        {
            Core.Register.Registers.R1,
            Core.Register.Registers.R2,
            Core.Register.Registers.R3,
            Core.Register.Registers.R4,
            Core.Register.Registers.R5,
            Core.Register.Registers.R6,
            Core.Register.Registers.R7,
            Core.Register.Registers.R8,
            Core.Register.Registers.IP
        };

        /// <summary>
        /// A list of registers to be restored from the stack when
        /// exiting a subroutine.
        /// </summary>
        private readonly Registers[] _stateRegistersRestore =
        {
            Core.Register.Registers.IP,
            Core.Register.Registers.R8,
            Core.Register.Registers.R7,
            Core.Register.Registers.R6,
            Core.Register.Registers.R5,
            Core.Register.Registers.R4,
            Core.Register.Registers.R3,
            Core.Register.Registers.R2,
            Core.Register.Registers.R1,
        };

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
        }

        /// <summary>
        /// Initialize the CPU.
        /// </summary>
        /// <param name="aMemSeqId">
        /// The sequence ID for the memory region containing the code.
        /// </param>
        /// <param name="aStartAddress">
        /// The address from which the execution should commence.
        /// </param>
        public void Initialize(int aMemSeqId, int aStartAddress)
        {
            MemExecutableSeqId = aMemSeqId;

            if (_minExecutableBound != -1)
            {
                SetStartAddress(aStartAddress);
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

            SetStartAddress(aStartAddress);
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
        /// <param name="aStartAddress">
        /// The address from which the execution should commence.
        /// </param>
        public void SetStartAddress(int aStartAddress)
        {
            var baseMemRegion =
                Vm.Memory.GetMemoryRegion(MemExecutableSeqId);
            if (baseMemRegion is null)
            {
                throw new Exception();
            }

            // Offset the starting address by the base
            // size of the memory. This is the area of memory
            // containing the system memory and stack.
            var offsetAddress = baseMemRegion.Start + aStartAddress;
            if (offsetAddress < 0 || offsetAddress >= Vm.Memory.Length)
            {
                throw new IndexOutOfRangeException
                (
                    "SetStartAddress: starting position is " +
                    "outside of the data bounds."
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
            catch (Exception)
            {
                SetHaltedState(true);
                throw;
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
            catch (Exception)
            {
                SetHaltedState(true);
                throw;
            }

            // Advance the instruction pointer by the number of bytes
            // corresponding sum of the size of the arguments (in bytes).
            // Note: the IP register can also be changed upon
            // executing the instruction, see the comment below.
            Registers[_ipUserTuple] += (pos - iPos);

            if (ExecuteInstruction(ins, asmIns))
            {
                SetHaltedState(true);
            }

            // Have we executed everything that needs to be executed?
            // We need to check against the IP register here and not
            // the "pos" variable as the register can be changed
            // while executing certain instructions (e.g. ret).
            if (Registers[_ipUserTuple] >= _maxExecutableBound)
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
            catch (KeyNotFoundException)
            {
                SetHaltedState(true);
                throw new InvalidRegisterException();
            }
            catch (Exception)
            {
                SetHaltedState(true);
                throw;
            }
        }

        public void PushState()
        {
            // Push the state registers to the stack.
            foreach (var reg in _stateRegistersSave)
            {
                Vm.Memory.StackPushInt(Registers[reg]);
            }

            // Push the current stack frame size.
            Vm.Memory.StackPushInt(StackFrameSize);

            // Update the frame pointer to the current stack
            // pointer location.
            Registers[_fpSystemTuple] = Registers[_spSystemTuple];

            // Reset the stack frame size so that we can track
            // the stack frames as above again.
            StackFrameSize = 0;
        }

        public void PopState()
        {
            var framePointerAddress =
                Registers[_fpSystemTuple];

            // Reset the stack pointer to the address of the frame
            // pointer. Any stack entries higher in the stack
            // can be disregarded as they are out of scope
            // from this point forward.
            Registers[_spSystemTuple] = framePointerAddress;

            // Pop the old stack frame size.
            StackFrameSize =
                Vm.Memory.StackPopInt();

            // We will need to keep this for resetting
            // the frame pointer position below.
            var stackFrameSize = StackFrameSize;

            // Pop the state registers from the stack.
            foreach (var reg in _stateRegistersRestore)
            {
                Registers[reg] =
                    Vm.Memory.StackPopInt();
            }

            // Clear the arguments from the stack.
            var argCount = Vm.Memory.StackPopInt();
            for (var i = 0; i < argCount; i++)
            {
                Vm.Memory.StackPopInt();
            }

            // Adjust our frame pointer position to the original
            // frame pointer address plus the stack frame size.
            Registers[_fpSystemTuple] =
                framePointerAddress + stackFrameSize;
        }

        /// <summary>
        /// Read an opcode instruction argument from memory.
        /// </summary>
        /// <param name="aPos">
        /// The position in memory from which to begin 
        /// reading the argument.
        /// </param>
        /// <param name="aT">
        /// The type of the argument to be read.
        /// </param>
        /// <returns>
        /// An object containing the opcode instruction data.
        /// </returns>
        public object GetNextInstructionArgument(ref int aPos, Type aT)
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

                case { } when aT == typeof(InstructionSizeHint):
                    arg =
                        Vm.Memory.GetSizeHintIdent(aPos, UserCtx, true);
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
        /// Enable or disable the halted state of the CPU.
        /// </summary>
        /// <param name="aState">
        /// The halt state to apply to the CPU.
        /// </param>
        public void SetHaltedState(bool aState)
        {
            IsHalted = aState;
        }

        /// <summary>
        /// Clear any breakpoint trigger hooks that have been set.
        /// </summary>
        private void ClearBreakpoints()
        {
            Vm.Debugger.RemoveAllBreakpoints();
        }

        /// <summary>
        /// Reset the stack pointer to the bottom of the stack
        /// memory region.
        /// </summary>
        private void ResetStackPointer()
        {
            // Reset the stack pointer to the bottom of the
            // stack memory region.
            Registers[_spSystemTuple] = Vm.Memory.StackEnd;
        }
    }
}
