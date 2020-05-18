using System;
using System.Collections.Generic;
using System.Diagnostics;
using VMCore;
using VMCore.VM;
using VMCore.Assembler;
using System.Linq;
using VMCore.VM.Core;

namespace TestConsole
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            int mainMemoryCapacity = 2048;
            int stackCapacity = 100;
            int stackStart = mainMemoryCapacity;

            var program = new QuickIns[]
            {
                /*new QuickIns(OpCode.MOV_LIT_MEM,
                 *             new object[] { 3141, 13 }),
                new QuickIns(OpCode.MOV_LIT_REG,
                             new object[] { 5, Registers.R1 }),
                new QuickIns(OpCode.MOV_LIT_REG,
                             new object[] { 2, Registers.R2 }),
                new QuickIns(OpCode.MOV_LIT_EXP_MEM_REG,
                             new object[] { "(R1 * R2) + $3", Registers.R3 }),
                new QuickIns(OpCode.MOV_LIT_EXP_MEM_REG
                             new object[] { "(5 * 2) + 3", Registers.R4 }),
                new QuickIns(OpCode.HLT),
                // Does not execute but should show is the disassembly
                // output.
                new QuickIns(OpCode.MOV_LIT_REG,
                             new object[] { 0x13, Registers.R1 }),
                new QuickIns(OpCode.HLT),*/

                // Jump testing.
                new QuickIns(OpCode.MOV_LIT_REG,
                             new object[] { 100, Registers.R1 }),
                new QuickIns(OpCode.MOV_LIT_REG,
                             new object[] { 100, Registers.R2 }),
                new QuickIns(OpCode.MOV_LIT_REG,
                             new object[] { 100, Registers.R3 }),           // ACC check against R3.
                new QuickIns(OpCode.ADD_REG_REG,
                             new object[] { Registers.R1, Registers.R2 }),  // ACC == 200

                new QuickIns(OpCode.JNE_REG,
                             new object[] { Registers.R3, 0 },
                             new AsmLabel("GOOD", 1)),                      // Jump to #1

                new QuickIns(OpCode.MOV_LIT_REG,
                             new object[] { 314159, Registers.R4 }),
                new QuickIns(OpCode.HLT),
                new QuickIns(OpCode.LABEL, new object[] { "GOOD" }),                    
                new QuickIns(OpCode.MOV_LIT_REG,
                             new object[] { 951431, Registers.R4 }),        // #1
                new QuickIns(OpCode.HLT),
            };

            var vm = 
                new VirtualMachine(mainMemoryCapacity,
                                   stackCapacity);

            // Break point stuff for experimenting.
            /*Breakpoint.BreakpointAction ipBP = delegate (int x)
            {
                Debug.WriteLine($"Instruction Pointer breakpoint = {x}");
                return false;
            };

            // Break at IP = 9
            vm.Debugger.AddBreakpoint(9, Breakpoint.BreakpointType.IP, ipBP);


            Breakpoint.BreakpointAction pcBP = delegate (int x)
            {
                Debug.WriteLine($"Program Counter breakpoint = {x}");
                return false;
            };

            // Break at PC = 1
            vm.Debugger.AddBreakpoint(2, Breakpoint.BreakpointType.PC, pcBP);*/

            // Enable CPU debug logging.
            vm.CPU.SetLoggingEnabled(true);

            var programBytes = Utils.QuickRawCompile(program, true);
            //File.WriteAllBytes(@"D:\Downloads\test.bin", programBytes);

            /*Stopwatch sw1 = new Stopwatch();
            Stopwatch sw2 = new Stopwatch();

            sw1.Start();

            for (int i = 0; i < 1_000_000; i++)
            {
            }

            sw1.Stop();

            sw2.Start();

            for (int i = 0; i < 1_000_000; i++)
            {
            }

            sw2.Stop();

            Console.WriteLine("Elapsed 1 = {0}", sw1.Elapsed);
            Console.WriteLine("Elapsed 2 = {0}", sw2.Elapsed);
            return;*/

            vm.Run(programBytes);

            Console.WriteLine("----------[Registers]----------");
            vm.CPU.Registers.PrintRegisters();

            // TODO - show stack memory here when stack is done.

            Console.WriteLine("----------[Raw Memory]----------");
            var mem = vm.Memory.DirectGetMemoryRaw(0, 0x20);
            foreach (var m in mem)
            {
                Console.Write(m.ToString("X2") + " ");
            }

            Console.WriteLine();

            Console.WriteLine("----------[Disassembly]----------");
            foreach (var s in 
                     vm.CPU.Disassemble(vm.CPU.MemExecutableSeqID, true))
            {
                Console.WriteLine(s);
            }

            Console.ReadLine();
        }
    }
}
