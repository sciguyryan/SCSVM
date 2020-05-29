using VMCore.VM;

namespace UnitTests.Core.Memory.Helpers
{
    public class TestMemoryBase
    {
        protected int MainMemoryCapacity = 2048;
        protected int StackCapacity = 100;
        protected int StackStart;

        protected VirtualMachine Vm;

        public TestMemoryBase()
        {
            StackStart = MainMemoryCapacity;

            // In general it should be safe to re-use this here.
            Vm = new VirtualMachine(MainMemoryCapacity, StackCapacity);
        }
    }
}
