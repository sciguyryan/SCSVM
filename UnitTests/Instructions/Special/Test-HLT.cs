using System.Collections.Generic;
using VMCore;
using VMCore.VM;
using VMCore.Assembler;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Instructions
{
    [TestClass]
    public class Test_HLT
        : Test_Instruction_Base
    {
        public Test_HLT()
        {
        }

        /// <summary>
        /// Test is the CPU has it's internal halted state set after executing
        /// the halt instruction.
        /// </summary>
        [TestMethod]
        public void TestCPUHaltState()
        {
            var program = new QuickIns[]
            {
                // Attempt to write a value to a protected write register.
                new QuickIns(OpCode.HLT)
            };

            _vm.Run(Utils.QuickRawCompile(program));

            // Check if the halted state is set.
            Assert.IsTrue(_vm.CPU.IsHalted);
        }

        /// <summary>
        /// Test is the CPU has actually halted.
        /// </summary>
        [TestMethod]
        public void TestCPUIsHalted()
        {
            var expected = 0x123;

            var program = new QuickIns[]
            {
                new QuickIns(OpCode.MOV_LIT_REG, new object[] { expected, (byte)Registers.R1 }),
                new QuickIns(OpCode.HLT),
                // This statement should never execute.
                new QuickIns(OpCode.MOV_LIT_REG, new object[] { 0xABC, (byte)Registers.R1 })
            };

            _vm.Run(Utils.QuickRawCompile(program));

            // If the CPU halted after executing the HLT instruction
            // then the register R1 should still be set to the value
            // of "expected". The last statement should not have executed.
            Assert.IsTrue(_vm.CPU.Registers[Registers.R1] == expected);
        }
    }
}
