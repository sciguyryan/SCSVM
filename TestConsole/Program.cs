using System;
using System.Diagnostics;
using VMCore.VM;
using VMCore.AsmParser;
using VMCore.VM.Core.Utilities;

namespace TestConsole
{
    internal class Program
    {
        private static void Main()
        {
            var lines = new[]
            {
                /*"mov $0x10, R1",
                "mov $0x11, R2",
                "mov $0x21, R3",
                "add R1, R2",
                "jne R3, @GOOD",
                "mov $0x3141, R4",
                "hlt",
                "@GOOD",
                "mov $0x1413, R4",
                "push R1",
                "push R2",
                "push R3",
                "push R4",
                "hlt"*/
                /*"mov $0x10, R1",
                "mov $0x11, R2",
                "mov $0x100, R3",
                "add R1, R2",
                "call @TESTER:",
                "mov $0x123, R5",
                "hlt",
                "@TESTER:",
                "mov $0x1, R1",
                "mov $0x2, R2",
                "add R1, R2",
                "ret"*/
                "push $0x321",
                "push $0x123",
                "push $0x213",
                "push $0 ; the number of arguments for the method",
                "call @TESTER:",
                "mov $0x123, R5",
                "hlt",
                "@TESTER:",
                "mov $0x1, R1",
                "mov $0x2, R2",
                "add R1, R2",
                "push $0xA",
                "push $0xB",
                "push $0xC",
                "ret"
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

            var vm = new VirtualMachine();

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
            vm.Cpu.SetLoggingEnabled(true);

            var programBytes =
                Utils.QuickRawCompile(p.Parse(progText),
                                      true);

            //var programBytes = Utils.QuickRawCompile(program, true);
            //File.WriteAllBytes(@"D:\Downloads\test.bin", programBytes);

            vm.Run(programBytes);

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
