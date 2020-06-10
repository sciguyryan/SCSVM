using System;
using System.Diagnostics;
using VMCore.Assembler;
using VMCore.AsmParser;
using VMCore.VM;
using VMCore.VM.Core.Breakpoints;
using VMCore.VM.Core.Utilities;
using System.IO;
using System.Reflection;

namespace TestConsole
{
    internal class Program
    {
        private static void Main()
        {
            var path =
                Path.Join(GetProgramDirectory(), "code.asm");

            var progText = File.ReadAllText(path);

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

            var sectionData = p.Parse(progText);

            var compiler = new Compiler(sectionData, null, true);

            var bin = new BinFile(compiler.Compile());

            //File.WriteAllBytes(@"D:\Downloads\test.bin", bin.Raw);

            //var programBytes =
            //    QuickCompile.RawCompile(sectionData, true);

            //var programBytes = QuickCompile.RawCompile(program, true);
            //File.WriteAllBytes(@"D:\Downloads\test.bin", programBytes);

            vm.Run(bin);

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
            vm.Disassembler.DisplayDisassembly(true, false, true);

            Console.ReadLine();
        }

        /// <summary>
        /// Get the current path of this application.
        /// </summary>
        /// <returns>
        /// A string giving the path to the directory of this application.
        /// </returns>
        public static string GetProgramDirectory()
        {
            var loc = Assembly.GetExecutingAssembly().Location;
            return Path.GetDirectoryName(loc) ?? string.Empty;
        }
    }
}
