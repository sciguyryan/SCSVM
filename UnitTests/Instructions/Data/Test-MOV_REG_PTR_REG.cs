using System.Collections.Generic;
using VMCore;
using VMCore.Assembler;
using VMCore.VM;
using VMCore.VM.Core.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Instructions
{
    [TestClass]
    public class Test_MOV_REG_PTR_REG : Test_Instruction_Base
    {
        public Test_MOV_REG_PTR_REG()
        {
        }

        /// <summary>
        /// Test indirect reading with valid positive offset.
        /// </summary>
        [TestMethod]
        public void TestUserAssemblyWriteWithPositiveOffset()
        {
            var r1 = Registers.R1;
            var r2 = Registers.R2;
            var r3 = Registers.R3;

            const int address = 0x15;
            const int expected = 0x12;

            var program = new List<QuickInstruction>
            {
                new QuickInstruction(OpCode.MOV_LIT_REG, new object[] { expected, r1 }),    // mov $0x12, R1
                new QuickInstruction(OpCode.MOV_REG_MEM, new object[] { r1, address }),     // mov R1, $0x15
                new QuickInstruction(OpCode.MOV_LIT_REG, new object[] { address, r2 }),     // mov $0x15, R2
                new QuickInstruction(OpCode.MOV_REG_PTR_REG, new object[] { r2, r3 })       // mov *R2, R3
            };

            _vm.Run(Utils.QuickRawCompile(program));

            Assert.IsTrue(_vm.CPU.Registers[r3] == expected);
        }

        /// <summary>
        /// Test indirect reading with invalid negative offset.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(MemoryOutOfRangeException))]
        public void TestUserAssemblyWriteWithNetSignedOffset()
        {
            var r1 = Registers.R1;
            var r2 = Registers.R2;
            var r3 = Registers.R3;

            const int expected = 0x12;

            var program = new List<QuickInstruction>
            {
                new QuickInstruction(OpCode.MOV_LIT_REG, new object[] { -0x1, r1 }),    // mov $-0x1, R1
                new QuickInstruction(OpCode.MOV_REG_PTR_REG, new object[] { r1, r2 })   // mov *R1, R2
            };


            _vm.Run(Utils.QuickRawCompile(program));

            Assert.IsTrue(_vm.CPU.Registers[r3] == expected);
        }
    }
}
