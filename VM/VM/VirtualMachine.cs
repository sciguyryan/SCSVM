﻿using System;
using VMCore.Assembler;
using VMCore.VM.Core;
using VMCore.VM.Core.Memory;
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
        public Memory Memory { get; set; }

        /// <summary>
        /// The CPU that has been assigned to this virtual
        /// machine instance.
        /// </summary>
        public Cpu Cpu { get;}

        /// <summary>
        /// The debugger that has been assigned to this
        /// virtual machine instance.
        /// </summary>
        public Debugger Debugger { get; }

        #endregion // Public Properties

        #region Private Properties

        private int _stackFrameSize = 0;

#if DEBUG
        private int _dbgMainMemoryCapacity;
        private int _dbgStackCapacity;
        private int _dbgFinalMemorySize;
#endif

        #endregion // Private Properties

        public VirtualMachine(int aMainMemoryCapacity = 2048,
                              int aStackCapacity = 100,
                              bool aCanCpuSwapMemoryRegions = false)
        {
            Debugger = new Debugger(this);

            Memory = new Memory(aMainMemoryCapacity, aStackCapacity);
            Cpu = new Cpu(this, aCanCpuSwapMemoryRegions);

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

            return seqId;
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

            /*System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            
            sw.Start();
            
            for (int i = 0; i < 1_000_000; i++)
            {
                Cpu.Run();
            }

            sw.Stop();

            System.Diagnostics.Debug.WriteLine("Elapsed={0}", sw.Elapsed);*/
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

            /*System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            
            sw.Start();
            
            for (int i = 0; i < 1_000_000; i++)
            {
                Cpu.Run();
            }

            sw.Stop();

            System.Diagnostics.Debug.WriteLine("Elapsed={0}", sw.Elapsed);*/
        }

#if DEBUG
        private void LoadRegisterTestData()
        {
            /*Cpu.Registers[Registers.R1] = _dbgMainMemoryCapacity;

            Cpu.Registers[Registers.R2] = _dbgMainMemoryCapacity - 1;

            Cpu.Registers[Registers.R3] = sizeof(int);*/
        }
#endif
    }
}
