using VMCore.VM;

namespace UnitTests.Expressions.Helpers
{
    public class TestExpressionBase
    {
        protected VirtualMachine Vm;

        public TestExpressionBase()
        {
            // In general it should be safe to re-use this here.
            Vm = new VirtualMachine();
        }
    }
}
