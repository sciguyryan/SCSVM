using VMCore.VM.Core;
using VMCore.VM.Core.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests.Core.Memory.Helpers;

namespace UnitTests.Core.Memory
{
    [TestClass]
    public class TestMemoryAccess
        : TestMemoryBase
    {
        private readonly int _executableStart;

        public TestMemoryAccess()
        {
            // Create a dummy program (lots of NOPs) so that we have
            // an executable memory region to work with.
            var dummy = 
                new byte[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            (_executableStart, _, _) = Vm.Memory.AddExMemory(dummy);
        }

        #region Read/Write Public Region Tests

        [TestMethod]
        public void TestUserWriteInPublicRegion()
        {
            Vm.Memory
                .SetInt(0, 1, SecurityContext.User, false);
        }

        [TestMethod]
        public void TestUserReadInPublicRegion()
        {
            _ = Vm.Memory.GetInt(0, SecurityContext.User, false);
        }

        [TestMethod]
        public void TestSystemWriteInPublicRegion()
        {
            Vm.Memory
                .SetInt(0, 1, SecurityContext.System, false);
        }

        [TestMethod]
        public void TestSystemReadInPublicRegion()
        {
            _ = Vm.Memory.GetInt(0, SecurityContext.System, false);
        }

        #endregion

        #region Read/Write Private Region Tests

        [TestMethod]
        [ExpectedException(typeof(MemoryAccessViolationException))]
        public void TestUserWriteInPrivateRegion()
        {
            Vm.Memory
                .SetInt(PrivateRegionStart, 1, SecurityContext.User, false);
        }

        [TestMethod]
        [ExpectedException(typeof(MemoryAccessViolationException))]
        public void TestUserReadInPrivateRegion()
        {
            _ = 
                Vm.Memory.GetInt(PrivateRegionStart, SecurityContext.User, false);
        }

        [TestMethod]
        public void TestSystemWriteInPrivateRegion()
        {
            Vm.Memory
                .SetInt(StackStart, 1, SecurityContext.System, false);
        }

        [TestMethod]
        public void TestSystemReadInPrivateRegion()
        {
            _ = 
                Vm.Memory.GetInt(StackStart, SecurityContext.System, false);
        }

        #endregion

        #region Read/Write Non Executable Region Tests

        [TestMethod]
        [ExpectedException(typeof(MemoryAccessViolationException))]
        public void TestUserWriteInNonExRegion()
        {
            Vm.Memory
                .SetInt(0, 1, SecurityContext.User, true);
        }

        [TestMethod]
        [ExpectedException(typeof(MemoryAccessViolationException))]
        public void TestUserReadInNonExRegion()
        {
            _ = Vm.Memory.GetInt(0, SecurityContext.User, true);
        }

        [TestMethod]
        [ExpectedException(typeof(MemoryAccessViolationException))]
        public void TestSystemWriteInNonExRegion()
        {
            Vm.Memory
                .SetInt(0, 1, SecurityContext.System, true);
        }

        [TestMethod]
        [ExpectedException(typeof(MemoryAccessViolationException))]
        public void TestSystemReadInNonExRegion()
        {
            _ = Vm.Memory.GetInt(0, SecurityContext.System, true);
        }

        #endregion

        #region Read/Write Executable Region Tests

        [TestMethod]
        public void TestUserWriteInPublicExRegion()
        {
            Vm.Memory
                .SetInt(_executableStart, 1, SecurityContext.User, true);
        }

        [TestMethod]
        public void TestUserReadInPublicExRegion()
        {
            _ = 
                Vm.Memory.GetInt(_executableStart,
                                  SecurityContext.User,
                                  true);
        }

        [TestMethod]
        public void TestSystemWriteInPublicExRegion()
        {
            Vm.Memory
                .SetInt(_executableStart,
                        1,
                        SecurityContext.System,
                        true);
        }

        [TestMethod]
        public void TestSystemReadInPublicExRegion()
        {
            _ = 
                Vm.Memory.GetInt(_executableStart,
                                  SecurityContext.System,
                                  true);
        }

        #endregion

        #region Read/Write Executable Cross Region Tests

        [TestMethod]
        [ExpectedException(typeof(MemoryAccessViolationException))]
        public void TestUserWriteInPublicExCrossRegion()
        {
            Vm.Memory
                .SetInt(_executableStart - 2,
                        1,
                        SecurityContext.User,
                        true);
        }

        [TestMethod]
        [ExpectedException(typeof(MemoryAccessViolationException))]
        public void TestUserReadInPublicExCrossRegion()
        {
            _ =
                Vm.Memory.GetInt(_executableStart - 2,
                                  SecurityContext.User,
                                  true);
        }

        [TestMethod]
        [ExpectedException(typeof(MemoryAccessViolationException))]
        public void TestSystemWriteInPublicExCrossRegion()
        {
            // This will fail as any attempt to read across
            // a memory region boundary will throw an exception,
            // even for a system-level context.
            Vm.Memory
                .SetInt(_executableStart - 2,
                        1,
                        SecurityContext.System,
                        true);
        }

        [TestMethod]
        [ExpectedException(typeof(MemoryAccessViolationException))]
        public void TestSystemReadInPublicExCrossRegion()
        {
            // This will fail as any attempt to read across
            // a memory region boundary will throw an exception,
            // even for a system-level context.
            _ =
                Vm.Memory.GetInt(_executableStart - 2,
                                  SecurityContext.System,
                                  true);
        }

        #endregion

        #region Read/Write Cross Region Tests

        [TestMethod]
        [ExpectedException(typeof(MemoryAccessViolationException))]
        public void TestUserWriteCrossRegion()
        {
            // 2 bytes of the data would be written into
            // a private write region.
            // This should not be permitted with a user
            // security context.
            var pos = StackStart - 2;

            Vm.Memory.SetInt(pos, 1, SecurityContext.User, false);
        }

        [TestMethod]
        [ExpectedException(typeof(MemoryAccessViolationException))]
        public void TestUserReadCrossRegion()
        {
            // 2 bytes of the data would be read from
            // a private write region.
            // This should not be permitted with a user
            // security context.
            var pos = PrivateRegionStart - 2;

            _ = Vm.Memory.GetInt(pos, SecurityContext.User, false);
        }

        [TestMethod]
        public void TestSystemWriteCrossRegion()
        {
            // This should succeed because we are attempting
            // to write between a PR/PW and a R/W region.
            // As the PR/PW region requires the highest permissions
            // those permissions would need to be met for
            // access to be granted.
            // As are using a system level context, those higher
            // permissions will be met.
            var pos = StackStart - 2;

            Vm.Memory.SetInt(pos, 1, SecurityContext.System, false);
        }

        [TestMethod]
        public void TestSystemReadCrossRegion()
        {
            // This should succeed because we are attempting
            // to read between a PR/PW and a R/W region.
            // As the PR/PW region requires the highest permissions
            // those permissions would need to be met for
            // access to be granted.
            // As are using a system level context, those higher
            // permissions will be met.
            var pos = StackStart - 2;

            _ = Vm.Memory.GetInt(pos, SecurityContext.System, false);
        }

        #endregion

        #region Read/Write Invalid Region Tests

        [TestMethod]
        [ExpectedException(typeof(MemoryOutOfRangeException))]
        public void TestUserWriteInvalidRegion()
        {
            Vm.Memory.SetInt(-1, 1, SecurityContext.User, false);
        }

        [TestMethod]
        [ExpectedException(typeof(MemoryOutOfRangeException))]
        public void TestUserReadInvalidRegion()
        {
            _ = Vm.Memory.GetInt(-1, SecurityContext.User, false);
        }

        [TestMethod]
        [ExpectedException(typeof(MemoryOutOfRangeException))]
        public void TestUserWritePartialInvalidRegion()
        {
            // 2 bytes will be written outside of memory bounds.
            var pos = Vm.Memory.Length - 2;

            Vm.Memory
                .SetInt(pos, 1, SecurityContext.User, false);
        }

        [TestMethod]
        [ExpectedException(typeof(MemoryOutOfRangeException))]
        public void TestUserReadPartialInvalidRegion()
        {
            // 2 bytes will be read from outside of memory bounds.
            var pos = Vm.Memory.Length - 2;

            _ = Vm.Memory.GetInt(pos, SecurityContext.User, false);
        }

        #endregion
    }
}
