using VMCore.Assembler;
using VMCore.VM;
using VMCore.VM.Core;
using VMCore.VM.Core.Exceptions;
using VMCore.VM.Core.Register;
using VMCore.VM.Core.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Core.Registers
{
    [TestClass]
    public class TestRegisterAccess
    {
        private readonly VirtualMachine _vm;

        public TestRegisterAccess()
        {
            // In general it should be safe to re-use this here.
            _vm = new VirtualMachine();

            // Add a dummy register to test access.
            var reg = 
                new Register(_vm.Cpu,
                      RegisterAccess.PR | RegisterAccess.PW);

            _vm.Cpu.Registers
                .Registers.Add(VMCore.VM.Core.Register.Registers.TESTER,
                               reg);
        }

        /// <summary>
        /// Test if writing to a protected write register throws an exception when
        /// run from user-code assembly.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(RegisterAccessViolationException))]
        public void TestUserAssemblyWriteProtectedRegister()
        {
            var program = new []
            {
                // Attempt to write a value to a protected write register.
                new QuickIns(OpCode.MOV_LIT_REG,
                             new object[] { 0x0, VMCore.VM.Core.Register.Registers.TESTER })
            };

            // This should fail with a RegisterAccessViolationException.
            _vm.Run(Utils.QuickRawCompile(program));
        }

        /// <summary>
        /// Test if writing to a protected write register throws
        /// an exception when run from code using a user security context.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(RegisterAccessViolationException))]
        public void TestUserDirectWriteProtectedRegister()
        {
            _vm.Cpu.Registers[(VMCore.VM.Core.Register.Registers.TESTER, SecurityContext.User)] = 0x1;
        }

        /// <summary>
        /// Test if writing to a protected write register does not throw an exception
        /// when run directly from normal code when using a system security context.
        /// </summary>
        [TestMethod]
        public void TestSystemDirectWriteProtectedRegister()
        {
            _vm.Cpu.Registers[(VMCore.VM.Core.Register.Registers.TESTER, SecurityContext.System)] = 0x1;
        }

        /// <summary>
        /// Test if reading from a protected-read register throws an exception when
        /// run from user-code assembly.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(RegisterAccessViolationException))]
        public void TestUserAssemblyReadProtectedRegister()
        {
            var program = new []
            {
                // Attempt to read a value from a protected write register
                // directly from user code.
                new QuickIns(OpCode.MOV_REG_MEM,
                             new object[] { VMCore.VM.Core.Register.Registers.TESTER, 0x0 })
            };

            // This should fail with a RegisterAccessViolationException.
            _vm.Run(Utils.QuickRawCompile(program));
        }

        /// <summary>
        /// Test if reading from a protected-read register throws
        /// an exception when run from code using a user security context.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(RegisterAccessViolationException))]
        public void TestUserDirectReadProtectedRegister()
        {
            _ = _vm.Cpu.Registers[(VMCore.VM.Core.Register.Registers.TESTER, SecurityContext.User)];
        }

        /// <summary>
        /// Test if reading from a protected-read register does not throw an exception
        /// when run directly from normal code when using a system security context.
        /// </summary>
        [TestMethod]
        public void TestSystemDirectReadProtectedRegister()
        {
            _ = _vm.Cpu.Registers[(VMCore.VM.Core.Register.Registers.SP, SecurityContext.System)];
        }

        /// <summary>
        /// Test if reading from a readable register does not throw an exception
        /// when run from user assembly code.
        /// </summary>
        [TestMethod]
        public void TestUserAssemblyReadRegister()
        {
            var program = new []
            {
                // Attempt to write a value to a protected write register.
                new QuickIns(OpCode.MOV_REG_MEM,
                             new object[] { VMCore.VM.Core.Register.Registers.R1, 0x0 })
            };

            // This should fail with a RegisterAccessViolationException.
            _vm.Run(Utils.QuickRawCompile(program));
        }

        /// <summary>
        /// Test if reading from a readable register does not throw an exception
        /// when run directly from normal code when using a user security context.
        /// </summary>
        [TestMethod]
        public void TestUserDirectReadRegister()
        {
            _ = _vm.Cpu.Registers[(VMCore.VM.Core.Register.Registers.R1, SecurityContext.User)];
        }

        /// <summary>
        /// Test if reading from a readable register does not throw an exception
        /// when run directly from normal code when using a system security context.
        /// </summary>
        [TestMethod]
        public void TestSystemDirectReadRegister()
        {
            _ = _vm.Cpu.Registers[(VMCore.VM.Core.Register.Registers.R1, SecurityContext.System)];
        }

        /// <summary>
        /// Test that reading a value from an invalid register
        /// via assembly throws an exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidRegisterException))]
        public void TestAssemblyReadInvalidRegister()
        {
            var program = new []
            {
                new QuickIns(OpCode.MOV_REG_MEM,
                             new object[] { (VMCore.VM.Core.Register.Registers)0xFF, 0x0 }),
            };

            // This should throw an exception as the specified register
            // does not exist in the Registers enum.
            _vm.Run(Utils.QuickRawCompile(program));
        }

        /// <summary>
        /// Test that writing a value from an invalid register
        /// throws an exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidRegisterException))]
        public void TestAssemblyWriteInvalidRegister()
        {
            var program = new []
            {
                new QuickIns(OpCode.MOV_LIT_REG,
                             new object[] { 0x123, (VMCore.VM.Core.Register.Registers)0xFF }),
            };

            // This should throw an exception as the specified register
            // does not exist in the Registers enum.
            _vm.Run(Utils.QuickRawCompile(program));
        }
    }
}
