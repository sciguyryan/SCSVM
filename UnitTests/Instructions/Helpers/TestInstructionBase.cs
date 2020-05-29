using VMCore.VM;

namespace UnitTests.Instructions.Helpers
{
    public class TestInstructionBase
    {
        protected VirtualMachine Vm;

        public TestInstructionBase()
        {
            // In general it should be safe to re-use this here.
            Vm = new VirtualMachine();
        }
    }
}
