using System;
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
        public RawBinaryFile Assembly { get; set; }

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

        public VirtualMachine(int mainMemoryCapacity = 2048, int stackCapacity = 100)
        {
            CPU = new CPU(this);
            Debugger = new Debugger(this);

            // The final memory size is equal to the base memory capacity
            // plus the stack capacity multiplied by the size of an integer.
            var finalMemorySize = mainMemoryCapacity + (stackCapacity * sizeof(int));
            Memory = new Memory(finalMemorySize);

#if DEBUG
            _dbgMainMemoryCapacity = mainMemoryCapacity;
            _dbgStackCapacity = stackCapacity;
            _dbgFinalMemorySize = finalMemorySize;
#endif

            // The region directly after the main memory
            // is reserved for the stack memory.
            // The stack memory region should be marked
            // as no read/write as the only methods
            // accessing or modifying it should be system only.
            var stackStart = mainMemoryCapacity;
            var stackEnd = finalMemorySize - 1;
            Memory.AddMemoryRegion(stackStart, stackEnd, MemoryAccess.PR | MemoryAccess.PW);

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
        /// <param name="startAddress">The starting address from which to begin execution of the program.</param>
        public void Run(int startAddress = 0)
        {
            if (Assembly == null)
            {
                throw new Exception("Run: no assembly file loaded.");
            }

            CPU.Reset();

            Run(Assembly[RawBinarySections.Code].Raw, startAddress);
        }

        /// <summary>
        /// Run a bytecode program to completion.
        /// </summary>
        /// <param name="raw">The raw bytecode data representing the program.</param>
        /// <param name="startAddress">The starting address from which to begin execution of the program.</param>
        public void Run(byte[] raw, int startAddress = 0)
        {
            if (raw.Length == 0)
            {
                throw new Exception("Run: no byte code provided.");
            }

            CPU.Reset();

#if DEBUG
            // This should be done after reset
            // as to avoid the possibility of the
            // data being overwritten.
            LoadRegisterTestData();
#endif

            CPU.LoadData(raw, startAddress);
            CPU.Run();
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

        private List<byte[]> GetStackRange(int start, int count)
        {
            throw new NotImplementedException();
        }
    }
}