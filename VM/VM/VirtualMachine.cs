#nullable enable

using System;
using System.Diagnostics;
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
        /// The assembly binary file that has been loaded into this
        /// virtual machine instance.
        /// </summary>
        public BinFile Assembly { get; set; }

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

        public VirtualMachine(int aMainMemoryCapacity = 4096,
                              int aStackCapacity = 100,
                              bool aCanCpuSwapMemoryRegions = false)
        {
            Debugger = new Debugger(this);

            Memory = 
                new Memory(this, aMainMemoryCapacity, aStackCapacity);

            Cpu = new Cpu(this, aCanCpuSwapMemoryRegions);

            Disassembler = new Disassembler(this);

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
        /// Load a binary into memory and initialize the CPU.
        /// </summary>
        /// <param name="aRaw">
        /// The raw byte code data representing the program.
        /// </param>
        /// <param name="aStartAddr">
        /// The starting address from which to begin 
        /// the execution of the program.
        /// </param>
        /// <param name="aCanSwapMemoryRegions">
        /// A boolean, true if the CPU will be permitted to swap between
        /// executable memory regions, false otherwise.
        /// </param>
        /// <returns>
        /// The sequence ID of the executable memory region.
        /// </returns>
        public int LoadAndInitialize(byte[] aRaw,
                                     int aStartAddr = 0,
                                     bool aCanSwapMemoryRegions = true)
        {
            if (aRaw.Length == 0)
            {
                throw new Exception("Initialize: no byte code provided.");
            }

            // In case we have used this virtual machine
            // instance before.
            Memory.RemoveExecutableRegions();

            // Clear any data within the CPU.
            Cpu.Reset();

#if DEBUG
            // This should be done after reset
            // as to avoid the possibility of the
            // data being overwritten.
            LoadRegisterTestData();
#endif

            // Load the executable data into memory.
            var (_, _, seqId) =
                Memory.AddExMemory(aRaw);

            Cpu.Initialize(seqId, aStartAddr);

            // Load any break point observers that have been
            // specified.
            SetBreakpointObservers();

            return seqId;
        }


        public int LoadAndInitialize(BinFile aBinary,
                                     int aStartAddr = 0,
                                     bool aCanSwapMemoryRegions = true)
        {
            if (aBinary == null)
            {
                throw new Exception("Initialize: no byte code provided.");
            }

            // In case we have used this virtual machine
            // instance before.
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

            foreach (var s in aBinary.Sections)
            {
                switch (s.SectionId)
                {
                    case BinSections.Text:
                        codeSection = s;
                        break;

                    case BinSections.Data:
                        dataSection = s;
                        break;

                    case BinSections.Meta:
                        break;

                    case BinSections.RData:
                        break;

                    case BinSections.BSS:
                        break;

                    case BinSections.SectionData:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (codeSection is null)
            {
                throw new Exception("Bad binary file.");
            }

            // Load the instruction data into memory.
            var (insStart, insEnd, insSeqId) =
                Memory.AddExMemory(codeSection.Raw);

            //Debug.WriteLine("In VM: " + string.Join(", ", codeSection.Raw));
            //Debug.WriteLine($"Added instruction section at {insStart}, {insEnd}. SeqID = {insSeqId}");

            // If we have a data section then load that into
            // it's own memory section.
            // Load the instruction data into memory.
            if (!(dataSection is null))
            {
                var (dataStart, dataEnd, dataSeqId) =
                    Memory.AddExMemory(dataSection.Raw);
                //Debug.WriteLine($"Added data section at {dataStart}, {dataEnd}. SeqID = {dataSeqId}");
            }

            //Debug.WriteLine(string.Join(", ", Memory.DirectGetMemoryRaw(insStart, insEnd)));

            Cpu.Initialize(insSeqId, startAddr);

            // Load any break point observers that have been
            // specified.
            SetBreakpointObservers();

            return insSeqId;
        }

        /// <summary>
        /// Run a byte code program to completion.
        /// </summary>
        /// <param name="aRaw">
        /// The raw byte code data representing the program.
        /// </param>
        /// <param name="aStartAddr">
        /// The starting address from which to begin 
        /// the execution of the program.
        /// </param>
        /// <param name="aCanSwapMemoryRegions">
        /// A boolean, true if the CPU will be permitted to swap between
        /// executable memory regions, false otherwise.
        /// </param>
        public void Step(byte[] aRaw,
                         int aStartAddr = 0,
                         bool aCanSwapMemoryRegions = true)
        {
            LoadAndInitialize(aRaw, aStartAddr, aCanSwapMemoryRegions);

            Cpu.Step();
        }

        /// <summary>
        /// Run a byte code program to completion.
        /// </summary>
        /// <param name="aRaw">
        /// The raw byte code data representing the program.
        /// </param>
        /// <param name="aStartAddr">
        /// The starting address from which to begin 
        /// the execution of the program.
        /// </param>
        /// <param name="aCanSwapMemoryRegions">
        /// A boolean, true if the CPU will be permitted to swap between
        /// executable memory regions, false otherwise.
        /// </param>
        public void Run(byte[] aRaw,
                        int aStartAddr = 0,
                        bool aCanSwapMemoryRegions = true)
        {
            LoadAndInitialize(aRaw, aStartAddr, aCanSwapMemoryRegions);

            Cpu.Run();
        }

        public void Run(BinFile aBinary,
                        int aStartAddr = 0,
                        bool aCanSwapMemoryRegions = true)
        {
            LoadAndInitialize(aBinary, aStartAddr, aCanSwapMemoryRegions);

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
        public void PerformanceTest(string aInsStr,
                                    bool aOptimize = false)
        {
            var p = new AsmParser.AsmParser();

            var ins = 
                p.Parse(aInsStr).CodeSectionData.ToArray();

            var bytes = 
                QuickCompile.RawCompile(ins, aOptimize);

            var insCount = ins.Length;

            LoadAndInitialize(bytes, 0, false);

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
