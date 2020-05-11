using System.Collections.Generic;
using VMCore;
using VMCore.Assembler;
using VMCore.VM;
using VMCore.VM.Core.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Core.Reg
{
    [TestClass]
    public class TestRegisterAccess
    {
        private int _mainMemoryCapacity = 2048;
        private int _stackCapacity = 100;
        private int _stackStart;

        private VirtualMachine _vm;

        public TestRegisterAccess()
        {
            _stackStart = _mainMemoryCapacity;

            // In general it should be safe to re-use this here.
            _vm = new VirtualMachine(_mainMemoryCapacity, _stackCapacity);
        }

        /// <summary>
        /// Test if writing to a protected write register throws an exception when
        /// run from user-code assembly.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(RegisterAccessViolationException))]
        public void TestUserAssemblyWriteProtectedRegister()
        {
            var program = new List<QuickIns>
            {
                // Attempt to write a value to a protected write register.
                new QuickIns(OpCode.MOV_LIT_REG, new object[] { 0x0, Registers.SP })
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
            _vm.CPU.Registers[(Registers.SP, SecurityContext.User)] = 0x1;
        }

        /// <summary>
        /// Test if writing to a protected write register does not throw an exception
        /// when run directly from normal code when using a system security context.
        /// </summary>
        [TestMethod]
        public void TestSystemDirectWriteProtectedRegister()
        {
            _vm.CPU.Registers[(Registers.SP, SecurityContext.System)] = 0x1;
        }

        /// <summary>
        /// Test if reading from a protected-read register throws an exception when
        /// run from user-code assembly.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(RegisterAccessViolationException))]
        public void TestUserAssemblyReadProtectedRegister()
        {
            var program = new List<QuickIns>
            {
                // Attempt to read a value from a protected write register
                // directly from user code.
                new QuickIns(OpCode.MOV_REG_MEM, new object[] { Registers.SP, 0x0 })
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
            _ = _vm.CPU.Registers[(Registers.SP, SecurityContext.User)];
        }

        /// <summary>
        /// Test if reading from a protected-read register does not throw an exception
        /// when run directly from normal code when using a system security context.
        /// </summary>
        [TestMethod]
        public void TestSystemDirectReadProtectedRegister()
        {
            _ = _vm.CPU.Registers[(Registers.SP, SecurityContext.System)];
        }

        /// <summary>
        /// Test if reading from a readable register does not throw an exception
        /// when run from user assembly code.
        /// </summary>
        [TestMethod]
        public void TestUserAssemblyReadRegister()
        {
            var program = new List<QuickIns>
            {
                // Attempt to write a value to a protected write register.
                new QuickIns(OpCode.MOV_REG_MEM, new object[] { Registers.R1, 0x0 })
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
            _ = _vm.CPU.Registers[(Registers.R1, SecurityContext.User)];
        }

        /// <summary>
        /// Test if reading from a readable register does not throw an exception
        /// when run directly from normal code when using a system security context.
        /// </summary>
        [TestMethod]
        public void TestSystemDirectReadRegister()
        {
            _ = _vm.CPU.Registers[(Registers.R1, SecurityContext.System)];
        }

        /// <summary>
        /// Test that reading a value from an invalid register
        /// via assembly throws an exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidRegisterException))]
        public void TestAssemblyReadInvalidRegiser()
        {
            var program = new List<QuickIns>
            {
                new QuickIns(OpCode.MOV_REG_MEM, new object[] { (Registers)0xFF, 0x0 }),
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
        public void TestAssemblyWriteInvalidRegiser()
        {
            var program = new List<QuickIns>
            {
                new QuickIns(OpCode.MOV_LIT_REG, new object[] { 0x123, (Registers)0xFF }),
            };

            // This should throw an exception as the specified register
            // does not exist in the Registers enum.
            _vm.Run(Utils.QuickRawCompile(program));
        }
    }
}
