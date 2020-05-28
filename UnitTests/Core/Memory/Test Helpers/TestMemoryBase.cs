using VMCore.VM;

namespace UnitTests.Memory
{
    public class Test_Memory_Base
    {
        protected int _mainMemoryCapacity = 2048;
        protected int _stackCapacity = 100;
        protected int _stackStart;

        protected VirtualMachine _vm;

        public Test_Memory_Base()
        {
            _stackStart = _mainMemoryCapacity;

            // In general it should be safe to re-use this here.
            _vm = new VirtualMachine(_mainMemoryCapacity, _stackCapacity);
        }
    }
}
