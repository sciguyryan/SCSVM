using System.Collections.Generic;
using VMCore;
using VMCore.Assembler;
using VMCore.VM;
using VMCore.VM.Core.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Instructions
{
    [TestClass]
    public class Test_MOV_REG_MEM : Test_Instruction_Base
    {
        public Test_MOV_REG_MEM()
        {
        }

        /// <summary>
        /// Test if copying an integer from a register to a valid 
        /// memory location succeeds when run from user-code assembly.
        /// </summary>
        [TestMethod]
        public void TestCopyRegisterToValidMemory()
        {
            const int expected = 0x123;

            var program = new List<QuickInstruction>
            {
                new QuickInstruction(OpCode.MOV_LIT_REG, new object[] { expected, Registers.R1 }),
                new QuickInstruction(OpCode.MOV_REG_MEM, new object[] { Registers.R1, 0x0 }),
            };

            _vm.Run(Utils.QuickRawCompile(program));

            // Extract the value type from memory.
            var intOut = _vm.Memory.GetValueAsType<int>(0x0, SecurityContext.System);

            Assert.IsTrue(intOut == expected);
        }

        /// <summary>
        /// Test if copying an integer from a register to an
        /// invalid memory location throws an exception
        /// when run from user-code assembly.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(MemoryOutOfRangeException))]
        public void TestCopyRegisterToInvalidMemory()
        {
            const int expected = 0x123;

            var program = new List<QuickInstruction>
            {
                new QuickInstruction(OpCode.MOV_LIT_REG, new object[] { expected, Registers.R1 }),
                new QuickInstruction(OpCode.MOV_REG_MEM, new object[] { Registers.R1, int.MaxValue }),
            };

            _vm.Run(Utils.QuickRawCompile(program));
        }
    }
}
