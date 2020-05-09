using System.Collections.Generic;
using VMCore;
using VMCore.Assembler;
using VMCore.VM;
using VMCore.VM.Core.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Instructions
{
    [TestClass]
    public class Test_MOV_LIT_MEM : Test_Instruction_Base
    {
        public Test_MOV_LIT_MEM()
        {
        }

        /// <summary>
        /// Test copying an integer to a valid memory address.
        /// </summary>
        [TestMethod]
        public void TestCopyRegisterToValidMemory()
        {
            const int expected = 0x123;

            var program = new List<QuickInstruction>
            {
                new QuickInstruction(OpCode.MOV_LIT_MEM, new object[] { expected, 0x0 }),
            };

            _vm.Run(Utils.QuickRawCompile(program));

            // Extract the value type from memory.
            var intOut = _vm.Memory.GetValueAsType<int>(0x0, SecurityContext.System);

            Assert.IsTrue(intOut == expected);
        }

        /// <summary>
        /// Test if copying an integer from a register to an
        /// invalid memory location throws an exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(MemoryOutOfRangeException))]
        public void TestCopyRegisterToInvalidMemory()
        {
            var program = new List<QuickInstruction>
            {
                new QuickInstruction(OpCode.MOV_LIT_MEM, new object[] { 0x00, int.MaxValue }),
            };

            _vm.Run(Utils.QuickRawCompile(program));
        }
    }
}
