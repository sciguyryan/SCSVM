using System.IO;
using VMCore.Assembler;
using VMCore.VM;

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

            var vm = new VirtualMachine();

            var bin = new BinFile(File.ReadAllBytes(args[0]));

            vm.Run(bin);
        }
    }
}
