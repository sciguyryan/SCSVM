#nullable enable

using System;
using System.Diagnostics;
using System.IO;
using VMCore.Assembler;
using VMCore.VM.Core.Breakpoints;
using VMCore.VM.Core.Memory;
using VMCore.VM.Core.Register;
using VMCore.VM.Core.Utilities;

namespace VMCore.VM
{
    public class VirtualMachine
    {
        #region Public Properties

        /// <summary>
        /// The binary file that has been loaded into this
        /// virtual machine instance.
        /// </summary>
        public BinFile? Binary { get; set; }

        /// <summary>
        /// The memory block that has been assigned to this
        /// virtual machine instance.
        /// </summary>
        public Memory Memory { get; }

        /// <summary>
        /// The CPU that has been assigned to this virtual
        /// machine instance.
        /// </summary>
        public Cpu Cpu { get; }

        /// <summary>
        /// The debugger that has been assigned to this
        /// virtual machine instance.
        /// </summary>
        public Debugger Debugger { get; }

        /// <summary>
        /// The disassembler that has been assigned to this
        /// virtual machine instance.
        /// </summary>
        public Disassembler Disassembler { get; }

        #endregion // Public Properties

        #region Private Properties

#if DEBUG
        private int _dbgMainMemoryCapacity;
        private int _dbgStackCapacity;
        private int _dbgFinalMemorySize;
#endif

        #endregion // Private Properties

        public VirtualMachine(int aMainMemoryCapacity = 64_000,
                              int aStackCapacity = 100,
                              bool aCanCpuSwapMemoryRegions = false,
                              BinFile? aBinary = null)
        {
            Debugger = new Debugger(this);

            Memory = 
                new Memory(this, aMainMemoryCapacity, aStackCapacity);

            Cpu = new Cpu(this, aCanCpuSwapMemoryRegions);

            Disassembler = new Disassembler(this);

            Binary = aBinary;

#if DEBUG
            _dbgMainMemoryCapacity = aMainMemoryCapacity;
            _dbgStackCapacity = aStackCapacity;
            _dbgFinalMemorySize = Memory.BaseMemorySize;
#endif

            // Build our instruction cache and apply and
            // hooks that we might need to use in the
            // execution of our program.
            ReflectionUtils.BuildCachesAndHooks();
        }

        /// <summary>
        /// Initialize the virtual machine.
        /// </summary>
        /// <param name="aBinary">
        /// The binary file to be executed.
        /// </param>
        /// <param name="aStartAddr">
        /// The starting address from which execution should begin.
        /// This address is relative to the entry point of the
        /// binary file within memory.
        /// </param>
        /// <param name="aCanSwapMemoryRegions">
        /// A boolean. True if the CPU should be permitted to swap
        /// memory regions, false otherwise.
        /// </param>
        /// <returns>
        /// The memory sequence ID for the region in which the
        /// executable code is allocated.
        /// </returns>
        public int LoadAndInitialize(BinFile aBinary,
                                     int aStartAddr = 0,
                                     bool aCanSwapMemoryRegions = true)
        {
            // Hold a reference to the binary as we might need
            // to use it again later.
            Binary = aBinary;

            // Clear any executable memory region that may
            // have been created. This needs to be done in
            // case we have used this virtual machine
            // instance prior.
            Memory.RemoveExecutableRegions();

            // Clear any data within the CPU.
            Cpu.Reset();

#if DEBUG
            // This should be done after reset
            // as to avoid the possibility of the
            // data being overwritten.
            LoadRegisterTestData();
#endif

            BinSection? codeSection = null;
            BinSection? dataSection = null;

            var startAddr = aStartAddr;

            // Iterate through the section list obtained
            // from the binary.
            foreach (var (id, sec) in aBinary.Sections)
            {
                switch (id)
                {
                    case BinSections.Text:
                        codeSection = sec;
                        break;

                    case BinSections.Data:
                        dataSection = sec;
                        break;

                    case BinSections.Meta:
                        break;

                    case BinSections.RData:
                        break;

                    case BinSections.BSS:
                        break;

                    case BinSections.SectionInfoData:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            // We must have an instruction code section.
            if (codeSection is null)
            {
                throw new InvalidDataException
                (
                    "LoadAndInitialize: binary file is invalid " +
                    " as it contains no instruction data section."
                );
            }

            // This is the address from which the binary file
            // should begin to be loaded.
            var entryAddress = aBinary.InitialAddress;

            // Load the instruction data section into memory.
            var (_, _, insSeqId) =
                Memory.AddExMemory(codeSection.Raw, entryAddress);

            if (!(dataSection is null))
            {
                // If we have a data section.
                // Load it into it's own memory region that
                // is contiguous with the main instruction
                // memory region.
                Memory.AddExMemory(dataSection.Raw,
                                   entryAddress + codeSection.Raw.Length);
            }

            // Initialize the CPU.
            Cpu.Initialize(insSeqId, startAddr);

            // Load any break point observers that have been
            // specified.
            SetBreakpointObservers();

            // Return the sequence ID for the region that contains
            // the instruction data.
            return insSeqId;
        }

        /// <summary>
        /// Initialize the virtual machine.
        /// </summary>
        /// <param name="aRawBytes">
        /// An array of bytes representing the binary file to be
        /// executed.
        /// </param>
        /// <param name="aStartAddr">
        /// The starting address from which execution should begin.
        /// This address is relative to the entry point of the
        /// binary file within memory.
        /// </param>
        /// <param name="aCanSwapMemoryRegions">
        /// A boolean. True if the CPU should be permitted to swap
        /// memory regions, false otherwise.
        /// </param>
        /// <returns>
        /// The memory sequence ID for the region in which the
        /// executable code is allocated.
        /// </returns>
        public int LoadAndInitialize(byte[] aRawBytes,
                                     int aStartAddr = 0,
                                     bool aCanSwapMemoryRegions = true)
        {
            return
                LoadAndInitialize(new BinFile(aRawBytes),
                    aStartAddr,
                    aCanSwapMemoryRegions);
        }

        /// <summary>
        /// Run a binary file to completion.
        /// </summary>
        /// <param name="aBinary">
        /// The binary file to be executed.
        /// </param>
        /// <param name="aStartAddr">
        /// The starting address from which execution should begin.
        /// This address is relative to the entry point of the
        /// binary file within memory.
        /// </param>
        /// <param name="aCanSwapMemoryRegions">
        /// A boolean. True if the CPU should be permitted to swap
        /// memory regions, false otherwise.
        /// </param>
        public void Run(BinFile aBinary,
                        int aStartAddr = 0,
                        bool aCanSwapMemoryRegions = true)
        {
            LoadAndInitialize(aBinary, aStartAddr, aCanSwapMemoryRegions);

            Cpu.Run();
        }

        /// <summary>
        /// Run a binary file to completion.
        /// </summary>
        /// <param name="aRawBytes">
        /// An array of bytes representing the binary file to be
        /// executed.
        /// </param>
        /// <param name="aStartAddr">
        /// The starting address from which execution should begin.
        /// This address is relative to the entry point of the
        /// binary file within memory.
        /// </param>
        /// <param name="aCanSwapMemoryRegions">
        /// A boolean. True if the CPU should be permitted to swap
        /// memory regions, false otherwise.
        /// </param>
        public void Run(byte[] aRawBytes,
                        int aStartAddr = 0,
                        bool aCanSwapMemoryRegions = true)
        {
            var bf = new BinFile(aRawBytes);
            LoadAndInitialize(bf, aStartAddr, aCanSwapMemoryRegions);

            Cpu.Run();
        }

        /// <summary>
        /// Sets any breakpoint trigger hooks that have been specified.
        /// </summary>
        private void SetBreakpointObservers()
        {
            if (Debugger.Breakpoints.Count == 0)
            {
                return;
            }

            const BreakpointType regRead = 
                BreakpointType.RegisterRead;

            const BreakpointType regWrite = 
                BreakpointType.RegisterWrite;

            const BreakpointType memRead =
                BreakpointType.MemoryRead;

            const BreakpointType memWrite =
                BreakpointType.MemoryWrite;

            // Add the handlers for each specified type.
            foreach (var bp in Debugger.Breakpoints)
            {
                Registers regId;

                switch (bp.Type)
                {
                    case BreakpointType.MemoryRead:
                        Memory.OnRead = aPos =>
                        {
                            var halt =
                                Debugger
                                    .TriggerBreakpoint(aPos, memRead);
                            Cpu.SetHaltedState(halt);
                        };
                        break;

                    case BreakpointType.MemoryWrite:
                        Memory.OnWrite = aPos =>
                        {
                            var halt =
                                Debugger
                                    .TriggerBreakpoint(aPos, memWrite);
                            Cpu.SetHaltedState(halt);
                        };
                        break;

                    case BreakpointType.RegisterRead:
                        if (bp.RegisterId is null)
                        {
                            continue;
                        }

                        regId = (Registers)bp.RegisterId;
                        Cpu.Registers.Registers[regId].OnRead = 
                            (aPos, aRegId) =>
                        {
                            var halt =
                                Debugger
                                    .TriggerBreakpoint(aPos, regRead, aRegId);
                            Cpu.SetHaltedState(halt);
                        };
                        break;

                    case BreakpointType.RegisterWrite:
                        if (bp.RegisterId is null)
                        {
                            continue;
                        }

                        regId = (Registers)bp.RegisterId;
                        Cpu.Registers.Registers[regId].OnRead = 
                            (aPos, aRegId) =>
                        {
                            var halt =
                                Debugger
                                    .TriggerBreakpoint(aPos, regWrite, aRegId);
                            Cpu.SetHaltedState(halt);
                        };
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// Remove any breakpoints currently hooked within this
        /// virtual machine instance.
        /// </summary>
        public void ClearBreakpoints()
        {
            Debugger.RemoveAllBreakpoints();
        }

#if DEBUG
        /// <summary>
        /// Run a performance test on a given code sample.
        /// </summary>
        /// <param name="aInsStr">
        /// The assembly string to be used for the test.
        /// </param>
        /// <param name="aOptimize">
        /// If the compiler should attempt to optimize the
        /// code.
        /// </param>
        public void PerformanceTest(string aInsStr,
                                    bool aOptimize = false)
        {
            var p = new AsmParser.AsmParser();

            var compSecs = p.Parse(aInsStr);

            var bf = 
                QuickCompile.CompileToBinFile(compSecs, aOptimize);

            var insCount = compSecs.CodeSectionData.Count;

            LoadAndInitialize(bf, 0, false);

            const int iterations = 1_000_000;

            var sw = new Stopwatch();
            sw.Start();

            for (var i = 0; i < iterations; i++)
            {
                Cpu.Run();
            }

            sw.Stop();

            var itrPerSec =
                (iterations * insCount) / sw.Elapsed.TotalSeconds;

            Debug.WriteLine
            (
                $"Iterations: {iterations:N0}, " +
                $"Instructions: {insCount * iterations:N0}, " +
                $"Total Time: {sw.Elapsed}, " +
                $"Instructions/Second: {itrPerSec:N0}"
            );
        }

        private void LoadRegisterTestData()
        {
        }
#endif
    }
}
