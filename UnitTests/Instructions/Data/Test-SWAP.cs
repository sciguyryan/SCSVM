using VMCore;
using VMCore.Assembler;
using VMCore.VM.Core;
using VMCore.VM.Core.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests.Instructions.Helpers;

namespace UnitTests.Instructions.Data
{
    [TestClass]
    public class TestSwap
        : TestInstructionBase
    {
        public TestSwap()
        {
        }

        /// <summary>
        /// Test if swapping the contents of two registers
        /// operates as expected.
        /// </summary>
        [TestMethod]
        public void TestSwapRegisters()
        {
            const Registers r1 = Registers.R1;
            const Registers r2 = Registers.R2;
            const int expected1 = 0x123;
            const int expected2 = 0x321;

            var program = new []
            {
                new QuickIns(OpCode.MOV_LIT_REG, 
                             new object[] { expected1, r1 }),
                new QuickIns(OpCode.MOV_LIT_REG, 
                             new object[] { expected2, r2 }),
                new QuickIns(OpCode.SWAP, 
                             new object[] { r1, r2 }),
            };

            Vm.Run(Utils.QuickRawCompile(program));

            Assert.IsTrue(Vm.Cpu.Registers[r1] == expected2);
            Assert.IsTrue(Vm.Cpu.Registers[r2] == expected1);
        }
    }
}
