using System.Collections.Generic;
using VMCore;
using VMCore.Assembler;
using VMCore.VM.Core.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Instructions
{
    [TestClass]
    public class Test_SWAP
        : Test_Instruction_Base
    {
        public Test_SWAP()
        {
        }

        /// <summary>
        /// Test if swapping the contents of two registers
        /// operates as expected.
        /// </summary>
        [TestMethod]
        public void TestSwapRegisters()
        {
            var r1 = Registers.R1;
            var r2 = Registers.R2;
            const int expected1 = 0x123;
            const int expected2 = 0x321;

            var program = new QuickIns[]
            {
                new QuickIns(OpCode.MOV_LIT_REG, new object[] { expected1, r1 }),
                new QuickIns(OpCode.MOV_LIT_REG, new object[] { expected2, r2 }),
                new QuickIns(OpCode.SWAP, new object[] { r1, r2 }),
            };

            _vm.Run(Utils.QuickRawCompile(program));

            Assert.IsTrue(_vm.Cpu.Registers[r1] == expected2);
            Assert.IsTrue(_vm.Cpu.Registers[r2] == expected1);
        }
    }
}
