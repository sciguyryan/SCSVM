using VMCore.Assembler;
using VMCore.VM.Core;
using VMCore.VM.Core.Register;
using VMCore.VM.Core.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests.Instructions.Helpers;

namespace UnitTests.Instructions.Special
{
    [TestClass]
    public class TestHlt
        : TestInstructionBase
    {
        public TestHlt()
        {
        }

        /// <summary>
        /// Test is the CPU has it's internal halted state set after executing
        /// the halt instruction.
        /// </summary>
        [TestMethod]
        public void TestCpuHaltState()
        {
            var program = new []
            {
                // Attempt to write a value to a protected write register.
                new CompilerIns(OpCode.HLT)
            };

            Vm.Run(QuickCompile.RawCompile(program));

            // Check if the halted state is set.
            Assert.IsTrue(Vm.Cpu.IsHalted);
        }

        /// <summary>
        /// Test is the CPU has actually halted.
        /// </summary>
        [TestMethod]
        public void TestCpuIsHalted()
        {
            const int expected = 0x123;

            var program = new []
            {
                new CompilerIns(OpCode.MOV_LIT_REG, 
                        new object[] { expected, (byte)Registers.R1 }),
                new CompilerIns(OpCode.HLT),
                // This statement should never execute.
                new CompilerIns(OpCode.MOV_LIT_REG, 
                        new object[] { 0xABC, (byte)Registers.R1 })
            };

            Vm.Run(QuickCompile.RawCompile(program));

            // If the CPU halted after executing the HLT instruction
            // then the register R1 should still be set to the value
            // of "expected". The last statement should not have executed.
            Assert.IsTrue(Vm.Cpu.Registers[Registers.R1] == expected);
        }
    }
}
