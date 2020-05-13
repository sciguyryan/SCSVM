using System;
using System.Collections.Generic;
using VMCore;
using VMCore.Assembler;
using VMCore.VM;
using VMCore.VM.Core.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Instructions
{
    [TestClass]
    public class Test_MOV_LIT_REG
        : Test_Instruction_Base
    {
        public Test_MOV_LIT_REG()
        {
        }

        /// <summary>
        /// Test if writing to a valid register succeeds when
        /// run from user-code assembly.
        /// </summary>
        [TestMethod]
        public void TestUserAssemblyWriteToRegister()
        {
            var register = Registers.R1;
            const int expected = 0x123;

            var program = new QuickIns[]
            {
                new QuickIns(OpCode.MOV_LIT_REG, new object[] { expected, register }),
            };


            _vm.Run(Utils.QuickRawCompile(program));

            Assert.IsTrue(_vm.CPU.Registers[register] == expected);
        }

        /// <summary>
        /// Test if writing to a valid register succeeds when
        /// run directly from code with a user context.
        /// </summary>
        [TestMethod]
        public void TestUserDirectWriteToRegister()
        {
            var register = Registers.R1;
            const int expected = 0x123;

            _vm.CPU.Registers[(register, SecurityContext.User)] = expected;

            Assert.IsTrue(_vm.CPU.Registers[register] == expected);
        }

        /// <summary>
        /// Test if writing to a valid register succeeds when
        /// run directly from code with a system context.
        /// </summary>
        [TestMethod]
        public void TestSystemDirectWriteToRegister()
        {
            var register = Registers.R1;
            const int expected = 0x123;

            _vm.CPU.Registers[(register, SecurityContext.System)] = expected;

            Assert.IsTrue(_vm.CPU.Registers[register] == expected);
        }
    }
}
