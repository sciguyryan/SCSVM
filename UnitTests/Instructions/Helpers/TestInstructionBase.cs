using VMCore.VM;
using VMCore.VM.Core;

namespace UnitTests.Instructions.Helpers
{
    public class TestInstructionBase
    {
        protected VirtualMachine Vm;

        protected SecurityContext UserCtx = SecurityContext.User;
        protected SecurityContext SystemCtx = SecurityContext.System;

        public TestInstructionBase()
        {
            // In general it should be safe to re-use this here.
            Vm = new VirtualMachine();
        }
    }
}
