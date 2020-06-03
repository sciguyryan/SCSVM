using VMCore.VM;
using VMCore.VM.Core.Memory;

namespace UnitTests.Core.Memory.Helpers
{
    public class TestMemoryBase
    {
        protected int MainMemoryCapacity = 2248;
        protected int StackCapacity = 100;
        protected int StackStart;

        protected int PublicRegionStart;
        protected int PublicRegionEnd;

        protected int PrivateRegionStart;
        protected int PrivateRegionEnd;

        protected VirtualMachine Vm;

        public TestMemoryBase()
        {
            StackStart = MainMemoryCapacity;

            // In general it should be safe to re-use this here.
            Vm = new VirtualMachine(MainMemoryCapacity, StackCapacity);

            // The stack memory region is always the last, by default.
            var len = Vm.Memory.StackStart;

            #region Public Region

            // Create a new public read/write region.

            PublicRegionStart = len;
            PublicRegionEnd = len + 100;

            const MemoryAccess publicAccess = 
                MemoryAccess.R | MemoryAccess.W;
            Vm.Memory.AddMemoryRegion(PublicRegionStart,
                                      PublicRegionEnd,
                                      publicAccess,
                                      "Public");

            #endregion // Public Region

            len += 100;

            #region Private Region

            // Create a new private read/write region.

            PrivateRegionStart = len;
            PrivateRegionEnd = len + 100;

            const MemoryAccess privateAccess = 
                MemoryAccess.PR | MemoryAccess.PW;
            Vm.Memory.AddMemoryRegion(PrivateRegionStart,
                                      PrivateRegionEnd,
                                      privateAccess,
                                      "Private");

            #endregion // Private Region
        }
    }
}
