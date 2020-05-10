using System.IO;
using VMCore;
using VMCore.Assembler;

namespace Runner
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                return;
            }

            var vm = new VirtualMachine
            {
                Assembly = BinFile.Load(File.ReadAllBytes(args[0]))
            };

            vm.Run();
        }
    }
}