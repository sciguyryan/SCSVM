using System.Collections.Generic;
using VMCore;
using VMCore.Assembler;
using VMCore.VM.Core.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Instructions
{
    [TestClass]
    public class Test_MOV_REG_REG
        : Test_Instruction_Base
    {
        public Test_MOV_REG_REG()
        {
        }

        /// <summary>
        /// Test if moving a value from a valid register to a
        /// valid register operates as expected.
        /// </summary>
        [TestMethod]
        public void TestUserAssemblyCopyValid()
        {
            const int expected = 0x12;

            var program = new QuickIns[]
            {
                new QuickIns(OpCode.MOV_LIT_REG, new object[] { expected, Registers.R1 }),
                new QuickIns(OpCode.MOV_REG_REG, new object[] { Registers.R1, Registers.R2 }),
            };

            _vm.Run(Utils.QuickRawCompile(program));

            Assert.IsTrue(_vm.Cpu.Registers[Registers.R2] == expected);
        }
    }
}
