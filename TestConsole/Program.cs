using System;
using System.Diagnostics;
using VMCore.VM;
using VMCore.AsmParser;
using VMCore.Expressions;
using VMCore.VM.Core.Utilities;

namespace TestConsole
{
    internal class Program
    {
        private static void Main()
        {
            const int mainMemoryCapacity = 2048;
            const int stackCapacity = 100;

            var lines = new string[]
            {
                "mov $0x10, R1",
                "mov $0x11, R2",
                "mov $0x100, R3",
                "add R1, R2",
                "jne R3, @GOOD",
                "mov $0x3141, R4",
                "hlt",
                "@GOOD",
                "mov $0x1413, R4",
                "hlt"
            };

            var progText =
                string.Join(Environment.NewLine, lines);

            var p = new AsmParser();

            /*var sw1 = new Stopwatch();
            var sw2 = new Stopwatch();
            const int iterations = 100_000;

            sw1.Start();

            for (var i = 0; i < iterations; i++)
            {
                p.Parse(progText);
            }

            sw1.Stop();

            sw2.Start();

            for (var i = 0; i < iterations; i++)
            {
            }

            sw2.Stop();

            Console.WriteLine("Elapsed 1 = {0}, {1} per iteration",
                              sw1.Elapsed,
                              sw1.Elapsed / iterations);
            Console.WriteLine("Elapsed 2 = {0}, {1} per iteration",
                              sw2.Elapsed,
                              sw2.Elapsed / iterations);
            return;*/

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

            // Enable Cpu debug logging.
            vm.Cpu.SetLoggingEnabled(true);

            var programBytes = 
                Utils.QuickRawCompile(p.Parse(progText),
                                      true);

            //var programBytes = Utils.QuickRawCompile(program, true);
            //File.WriteAllBytes(@"D:\Downloads\test.bin", programBytes);

            vm.Run(programBytes);

            vm.Memory.StackPushInt(0xAB);
            vm.Memory.StackPushInt(0xBA);

            Console.WriteLine("-------------[Registers]------------");
            vm.Cpu.Registers.PrintRegisters();

            Console.WriteLine("---------------[Stack]--------------");
            vm.Memory.PrintStack();

            /*Console.WriteLine("----------[Memory Regions]----------");
            var regionLines = 
                vm.Memory.GetFormattedMemoryRegions();
            foreach (var l in regionLines)
            {
                Console.WriteLine(l);
            }*/

            Console.WriteLine("------------[Raw Memory]------------");
            var mem = vm.Memory.DirectGetMemoryRaw(0, 0x20);
            foreach (var m in mem)
            {
                Console.Write(m.ToString("X2") + " ");
            }

            Console.WriteLine();

            Console.WriteLine("------------[Disassembly]-----------");
            foreach (var s in 
                     vm.Cpu.Disassemble(vm.Cpu.MemExecutableSeqId, true))
            {
                Console.WriteLine(s);
            }

            Console.ReadLine();
        }
    }
}
