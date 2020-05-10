using System.Collections.Generic;
using VMCore;
using VMCore.Assembler;
using VMCore.VM;
using VMCore.VM.Core.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Instructions
{
    [TestClass]
    public class Test_MOV_MEM_REG
        : Test_Instruction_Base
    {
        public Test_MOV_MEM_REG()
        {
        }

        /// <summary>
        /// Test copying an integer from a valid memory address.
        /// </summary>
        [TestMethod]
        public void TestCopyValidMemoryToRegister()
        {
            const int expected = 0x123;

            var program = new List<QuickIns>
            {
                new QuickIns(OpCode.MOV_LIT_MEM, new object[] { expected, 0x0 }),
                new QuickIns(OpCode.MOV_MEM_REG, new object[] { 0x0, Registers.R1 }),
            };

            _vm.Run(Utils.QuickRawCompile(program));

            Assert.IsTrue(_vm.CPU.Registers[Registers.R1] == expected);
        }

        /// <summary>
        /// Test if copying an integer from an invalid memory
        /// address throws an exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(MemoryOutOfRangeException))]
        public void TestCopyInvalidMemoryToRegister()
        {
            var program = new List<QuickIns>
            {
                new QuickIns(OpCode.MOV_MEM_REG, new object[] { int.MaxValue, Registers.R1 }),
            };

            _vm.Run(Utils.QuickRawCompile(program));
        }
    }
}
