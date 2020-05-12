﻿using System;
using System.Collections.Generic;
using VMCore.VM;
using VMCore.Assembler;
using VMCore.VM.Core.Mem;

namespace VMCore
{
    public class VirtualMachine
    {
        /// <summary>
        /// The assembly binary file that has been loaded into this
        /// virtual machine instance.
        /// </summary>
        public BinFile Assembly { get; set; }

        /// <summary>
        /// The memory block that has been assigned to this
        /// virtual machine instance.
        /// </summary>
        public Memory Memory { get; set; }

        /// <summary>
        /// The CPU that has been assigned to this virtual
        /// machine instance.
        /// </summary>
        public CPU CPU { get; private set; }

        /// <summary>
        /// The debugger that has been assigned to this
        /// virtual machine instance.
        /// </summary>
        public Debugger Debugger { get; private set; }

        private int _stackFrameSize = 0;

#if DEBUG
        private int _dbgMainMemoryCapacity;
        private int _dbgStackCapacity;
        private int _dbgFinalMemorySize;
#endif

        public VirtualMachine(int aMainMemoryCapacity = 2048,
                              int aStackCapacity = 100,
                              bool aCanCPUSwapMemoryRegions = false)
        {
            CPU = new CPU(this, aCanCPUSwapMemoryRegions);
            Debugger = new Debugger(this);

            // The final memory size is equal to the base memory capacity
            // plus the stack capacity multiplied by the size of an integer.
            var finalMemorySize = 
                aMainMemoryCapacity + (aStackCapacity * sizeof(int));
            Memory = new Memory(finalMemorySize);

#if DEBUG
            _dbgMainMemoryCapacity = aMainMemoryCapacity;
            _dbgStackCapacity = aStackCapacity;
            _dbgFinalMemorySize = finalMemorySize;
#endif

            // The region directly after the main memory
            // is reserved for the stack memory.
            // The stack memory region should be marked
            // as no read/write as the only methods
            // accessing or modifying it should be system only.
            var stackStart = aMainMemoryCapacity;
            var stackEnd = finalMemorySize - 1;
            Memory.AddMemoryRegion(stackStart,
                                   stackEnd,
                                   MemoryAccess.PR | MemoryAccess.PW);

            // Set the default stack pointer position to be at the very
            // end of our allocated memory block.
            CPU.Registers[(VMCore.Registers.SP, SecurityContext.System)] =
                finalMemorySize;

            // Build our instruction cache and apply and
            // hooks that we might need to use in the
            // execution of our program.
            ReflectionUtils.BuildCachesAndHooks();
        }

        /// <summary>
        /// Run the currently loaded binary file to completion.
        /// </summary>
        /// <param name="aStartAddress">The starting address from which to begin execution of the program.</param>
        public void Run(int aStartAddress = 0)
        {
            if (Assembly == null)
            {
                throw new Exception("Run: no assembly file loaded.");
            }

            CPU.Reset();

            Run(Assembly[BinSections.Code].Raw, aStartAddress);
        }

        /// <summary>
        /// Run a bytecode program to completion.
        /// </summary>
        /// <param name="aRaw">The raw bytecode data representing the program.</param>
        /// <param name="aStartAddress">The starting address from which to begin execution of the program.</param>
        public void Run(byte[] aRaw, int aStartAddress = 0, bool aCanSwapMemoryRegions = true)
        {
            if (aRaw.Length == 0)
            {
                throw new Exception("Run: no byte code provided.");
            }

            // In case we have used this virtual machine
            // instance before.
            Memory.RemoveExecutableRegions();

            // Clear any data within the CPU.
            CPU.Reset();

#if DEBUG
            // This should be done after reset
            // as to avoid the possibility of the
            // data being overwritten.
            LoadRegisterTestData();
#endif

            //CPU.LoadData(aRaw, aStartAddress);

            // Copy the data into memory.
            (_, _, int seqid) =
                Memory.SetupExMemory(aRaw);

            CPU.Run(seqid, 0);
            /*System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            
            sw.Start();
            
            for (int i = 0; i < 1_000_000; i++)
            {
                CPU.Run(seqid, 0);
            }

            sw.Stop();

            System.Diagnostics.Debug.WriteLine("Elapsed={0}", sw.Elapsed);*/
        }

#if DEBUG
        private void LoadRegisterTestData()
        {
            /*CPU.Registers[(VMCore.Registers.R1, SecurityContext.System)] 
                = _dbgMainMemoryCapacity;

            CPU.Registers[(VMCore.Registers.R2, SecurityContext.System)] 
                = _dbgMainMemoryCapacity - 1;

            CPU.Registers[(VMCore.Registers.R3, SecurityContext.System)] 
                = sizeof(int);*/
        }
#endif

        private List<byte[]> GetStackRange(int aStart, int aCount)
        {
            throw new NotImplementedException();
        }
    }
}