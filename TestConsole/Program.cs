using System;
using System.Collections.Generic;
using System.Diagnostics;
using VMCore;
using VMCore.VM;
using VMCore.Assembler;

namespace TestConsole
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            int mainMemoryCapacity = 2048;
            int stackCapacity = 100;
            int stackStart = mainMemoryCapacity;

            var program = new List<QuickIns>
            {
                /*// XOR flag testing.
                new QuickIns(OpCode.MOV_REG_REG, new object[] { Registers.R1, Registers.R2 }),
                new QuickIns(OpCode.XOR_REG_REG, new object[] { Registers.R1, Registers.R2 }),*/

                new QuickIns(OpCode.MOV_LIT_MEM, new object[] { 3141, 13 }),
                new QuickIns(OpCode.MOV_LIT_REG, new object[] { 5, Registers.R1 }),
                new QuickIns(OpCode.MOV_LIT_REG, new object[] { 2, Registers.R2 }),
                new QuickIns(OpCode.MOV_LIT_EXP_MEM_REG, new object[] { "(R1 * R2) + $3", Registers.R3 }),
                //new QuickInstruction(OpCode.MOV_LIT_EXP_MEM_REG, new object[] { "(5 * 2) + 3", Registers.R3 }),
                new QuickIns(OpCode.HLT),
                // Does not execute but should show is the disassembly
                // output.
                new QuickIns(OpCode.MOV_LIT_REG, new object[] { 0x13, Registers.R1 })
            };

            /*ass.Add(OpCode.LOAD, (int)Registers.DR1, 0x12);                 // mov 0x12, DR1
            ass.Add(OpCode.INC, (int)Registers.DR1);                        // inc DR1

            ass.Add(OpCode.LOAD, (int)Registers.DR2, 0x12);                 // mov 0x13, DR2

            ass.Add(OpCode.EQUAL, (int)Registers.DR1, (int)Registers.DR2);  // eq DR1, DR2
                                                                            // jmpe "BAD"        (does not specify a jump destination address directly)
                                                                            //                   (argument 0 is bound to the BAD label).
            var badLabelBind = new Dictionary<int, string>() { { 0, "BAD" } };
            ass.AddWithLabel(OpCode.JMPE, badLabelBind, -1);

            //ass.CreateLabel("GOOD");

            string good = "Good!\n\n";
            foreach (var c in good)
            {
                ass.Add(OpCode.PUSHL, c);
                ass.Add(OpCode.OUT,
                        (int)DeviceSockets.ConsoleControl, 
                        (int)ConsoleDevice.ControlCodes.WriteChar);         // out 0xDEF0, character
            }

            ass.Add(OpCode.HLT);                                            // hlt

            ass.CreateLabel("BAD");

            string bad = "Bad!\n\n";
            foreach (var c in bad)
            {
                ass.Add(OpCode.PUSHL, c);
                ass.Add(OpCode.OUT,
                        (int)DeviceSockets.ConsoleControl,
                        (int)ConsoleDevice.ControlCodes.WriteChar);         // out 0xDEF0, character
            }*/

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

            /*Stopwatch sw = new Stopwatch();

            sw.Start();

            for (int i = 0; i < 1_000_000; i++)
            {
                vm.Run(programBytes);
            }

            sw.Stop();

            Console.WriteLine("Elapsed={0}", sw.Elapsed);*/

            vm.Run(programBytes);

            Console.WriteLine("----------[Registers]----------");
            vm.CPU.Registers.PrintRegisters();

            // TODO - show stack memory here when stack is done.

            Console.WriteLine("----------[Raw Memory]----------");
            var mem = vm.Memory.GetValueRange(0, 0x20, false, SecurityContext.System);
            foreach (var m in mem)
            {
                Console.Write(m.ToString("X2") + " ");
            }

            Console.WriteLine();

            Console.WriteLine("----------[Disassembly]----------");
            foreach (var s in vm.CPU.Disassemble(vm.CPU.MemExecutableSeqID, true))
            {
                Console.WriteLine(s);
            }

            Console.ReadLine();
        }
    }
}
