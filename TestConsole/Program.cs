using System;
using System.Diagnostics;
using VMCore.VM;
using VMCore.AsmParser;
using VMCore.VM.Core;
using VMCore.VM.Core.Breakpoints;
using VMCore.VM.Core.Register;
using VMCore.VM.Core.Utilities;

namespace TestConsole
{
    internal class Program
    {
        private static void Main()
        {
            const int destOffset =
                sizeof(OpCode) * 8 +
                sizeof(int) * 7 +
                sizeof(Registers) * 1;

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
                ".section data",
                "str db 'Hello, world!',$0xA",
                "strLen equ $-str",
                ".section text",
                "push $0xAAA",  // Should remain in place once the stack is restored
                "push $0xC",    // TESTER Argument 3
                "push $0xB",    // TESTER Argument 2
                "push $0xA",    // TESTER Argument 1
                "push $3",      // The number of arguments for the subroutine
                "call !TESTER",
                "mov $0x123, R1",
                "hlt",

                "TESTER:",
                "mov $0x34, &FP, R3",
                "mov $0x30, &FP, R2",
                "mov $0x2C, &FP, R1",
                "add R1, R2",
                "add R3, AC",
                "push $0xCC",    // TESTER2 Argument 3
                "push $0xBB",    // TESTER2 Argument 2
                "push $0xAA",    // TESTER2 Argument 1
                "push $3",       // The number of arguments for the subroutine
                "call !TESTER2",
                "ret",

                "TESTER2:",
                "mov $0x34, &FP, R3",
                "mov $0x30, &FP, R2",
                "mov $0x2C, &FP, R1",
                "add R1, R2",
                "add R3, AC",
                "ret"
                
                /*"push $0xAAA",  // Should remain in place once the stack is restored
                "push $0xC",    // TESTER Argument 3
                "push $0xB",    // TESTER Argument 2
                "push $0xA",    // TESTER Argument 1
                "push $3",      // The number of arguments for the subroutine
                $"call &${destOffset}",
                //"call !TESTER",
                "mov $0x123, R1",
                "hlt",

                "TESTER:",
                "mov $0x34, &FP, R3",
                "mov $0x30, &FP, R2",
                "mov $0x2C, &FP, R1",
                "add R1, R2",
                "add R3, AC",
                "ret",*/
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

            /*Breakpoint.BreakpointAction regBp = delegate (int aX)
            {
                Debug.WriteLine($"Register breakpoint = {aX}");
                return false;
            };

            // Trigger breakpoint when the value 0x123 is written to R1.
            vm.Debugger.AddBreakpoint(0x123,
                                      BreakpointType.RegisterWrite,
                                      regBp,
                                      Registers.R1);

            Breakpoint.BreakpointAction memBP = delegate (int aX)
            {
                Debug.WriteLine($"Memory position {aX} was written too!");
                return false;
            };

            // Trigger breakpoint upon write to memory address 0x1.
            vm.Debugger.AddBreakpoint(0,
                                      BreakpointType.MemoryWrite,
                                      memBP);*/

            // Enable CPU debug logging.
            vm.Cpu.SetLoggingEnabled(true);

            var ins = 
                p.Parse(progText).CodeSectionData.ToArray();

            var programBytes =
                Utils.QuickRawCompile(ins, true);

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
            foreach (var s in vm.Disassembler.Disassemble(true))
            {
                Console.WriteLine(s);
            }

            Console.ReadLine();
        }
    }
}
