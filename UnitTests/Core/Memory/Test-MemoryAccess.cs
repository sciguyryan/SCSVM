using VMCore.VM;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VMCore.VM.Core.Exceptions;

namespace UnitTests.Memory
{
    [TestClass]
    public class Test_Memory_Access
        : Test_Memory_Base
    {
        private int _executableStart;

        public Test_Memory_Access()
        {
            // Create a dummy program so that we have
            // an executable memory region to work with.
            var dummy = 
                new byte[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            (_executableStart, _, _) = _vm.Memory.AddExMemory(dummy);
        }

        #region Read/Write Public Region Tests
        [TestMethod]
        public void TestUserWriteInPublicRegion()
        {
            _vm.Memory
                .SetInt(0, 1, SecurityContext.User, false);
        }

        [TestMethod]
        public void TestUserReadInPublicRegion()
        {
            _ = _vm.Memory.GetInt(0, SecurityContext.User, false);
        }

        [TestMethod]
        public void TestSystemWriteInPublicRegion()
        {
            _vm.Memory
                .SetInt(0, 1, SecurityContext.System, false);
        }

        [TestMethod]
        public void TestSystemReadInPublicRegion()
        {
            _ = _vm.Memory.GetInt(0, SecurityContext.System, false);
        }
        #endregion

        #region Read/Write Private Region Tests
        [TestMethod]
        [ExpectedException(typeof(MemoryAccessViolationException))]
        public void TestUserWriteInPrivateRegion()
        {
            _vm.Memory
                .SetInt(_stackStart, 1, SecurityContext.User, false);
        }

        [TestMethod]
        [ExpectedException(typeof(MemoryAccessViolationException))]
        public void TestUserReadInPrivateRegion()
        {
            _ = 
                _vm.Memory.GetInt(_stackStart, SecurityContext.User, false);
        }

        [TestMethod]
        public void TestSystemWriteInPrivateRegion()
        {
            _vm.Memory
                .SetInt(_stackStart, 1, SecurityContext.System, false);
        }

        [TestMethod]
        public void TestSystemReadInPrivateRegion()
        {
            _ = 
                _vm.Memory.GetInt(_stackStart, SecurityContext.System, false);
        }
        #endregion

        #region Read/Write Non Executable Region Tests
        [TestMethod]
        [ExpectedException(typeof(MemoryAccessViolationException))]
        public void TestUserWriteInNonEXRegion()
        {
            _vm.Memory
                .SetInt(0, 1, SecurityContext.User, true);
        }

        [TestMethod]
        [ExpectedException(typeof(MemoryAccessViolationException))]
        public void TestUserReadInNonEXRegion()
        {
            _ = _vm.Memory.GetInt(0, SecurityContext.User, true);
        }

        [TestMethod]
        [ExpectedException(typeof(MemoryAccessViolationException))]
        public void TestSystemWriteInNonEXRegion()
        {
            _vm.Memory
                .SetInt(0, 1, SecurityContext.System, true);
        }

        [TestMethod]
        [ExpectedException(typeof(MemoryAccessViolationException))]
        public void TestSystemReadInNonEXRegion()
        {
            _ = _vm.Memory.GetInt(0, SecurityContext.System, true);
        }
        #endregion

        #region Read/Write Executable Region Tests
        [TestMethod]
        public void TestUserWriteInPublicEXRegion()
        {
            _vm.Memory
                .SetInt(_executableStart, 1, SecurityContext.User, true);
        }

        [TestMethod]
        public void TestUserReadInPublicEXRegion()
        {
            _ = 
                _vm.Memory.GetInt(_executableStart,
                                  SecurityContext.User,
                                  true);
        }

        [TestMethod]
        public void TestSystemWriteInPublicEXRegion()
        {
            _vm.Memory
                .SetInt(_executableStart,
                        1,
                        SecurityContext.System,
                        true);
        }

        [TestMethod]
        public void TestSystemReadInPublicEXRegion()
        {
            _ = 
                _vm.Memory.GetInt(_executableStart,
                                  SecurityContext.System,
                                  true);
        }
        #endregion

        #region Read/Write Executable Cross Region Tests
        [TestMethod]
        [ExpectedException(typeof(MemoryAccessViolationException))]
        public void TestUserWriteInPublicEXCrossRegion()
        {
            _vm.Memory
                .SetInt(_executableStart - 2,
                        1,
                        SecurityContext.User,
                        true);
        }

        [TestMethod]
        [ExpectedException(typeof(MemoryAccessViolationException))]
        public void TestUserReadInPublicEXCrossRegion()
        {
            _ =
                _vm.Memory.GetInt(_executableStart - 2,
                                  SecurityContext.User,
                                  true);
        }

        [TestMethod]
        [ExpectedException(typeof(MemoryAccessViolationException))]
        public void TestSystemWriteInPublicEXCrossRegion()
        {
            // This will fail as any attempt to read across
            // a memory region boundary will throw an exception,
            // even for a system-level context.
            _vm.Memory
                .SetInt(_executableStart - 2,
                        1,
                        SecurityContext.System,
                        true);
        }

        [TestMethod]
        [ExpectedException(typeof(MemoryAccessViolationException))]
        public void TestSystemReadInPublicEXCrossRegion()
        {
            // This will fail as any attempt to read across
            // a memory region boundary will throw an exception,
            // even for a system-level context.
            _ =
                _vm.Memory.GetInt(_executableStart - 2,
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
            var pos = _stackStart - 2;

            _vm.Memory.SetInt(pos, 1, SecurityContext.User, false);
        }

        [TestMethod]
        [ExpectedException(typeof(MemoryAccessViolationException))]
        public void TestUserReadCrossRegion()
        {
            // 2 bytes of the data would be read from
            // a private write region.
            // This should not be permitted with a user
            // security context.
            var pos = _stackStart - 2;

            _ = _vm.Memory.GetInt(pos, SecurityContext.User, false);
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
            var pos = _stackStart - 2;

            _vm.Memory.SetInt(pos, 1, SecurityContext.System, false);
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
            var pos = _stackStart - 2;

            _ = _vm.Memory.GetInt(pos, SecurityContext.System, false);
        }
        #endregion

        #region Read/Write Invalid Region Tests
        [TestMethod]
        [ExpectedException(typeof(MemoryOutOfRangeException))]
        public void TestUserWriteInvalidRegion()
        {
            _vm.Memory.SetInt(-1, 1, SecurityContext.User, false);
        }

        [TestMethod]
        [ExpectedException(typeof(MemoryOutOfRangeException))]
        public void TestUserReadInvalidRegion()
        {
            _ = _vm.Memory.GetInt(-1, SecurityContext.User, false);
        }

        [TestMethod]
        [ExpectedException(typeof(MemoryOutOfRangeException))]
        public void TestUserWritePartialInvalidRegion()
        {
            // 2 bytes will be written outside of memory bounds.
            var pos = _vm.Memory.Length - 2;

            _vm.Memory
                .SetInt(pos, 1, SecurityContext.User, false);
        }

        [TestMethod]
        [ExpectedException(typeof(MemoryOutOfRangeException))]
        public void TestUserReadPartialInvalidRegion()
        {
            // 2 bytes will be read from outside of memory bounds.
            var pos = _vm.Memory.Length - 2;

            _ = _vm.Memory.GetInt(pos, SecurityContext.User, false);
        }
        #endregion
    }
}
