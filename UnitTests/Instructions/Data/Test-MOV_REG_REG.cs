using VMCore.Assembler;
using VMCore.VM.Core;
using VMCore.VM.Core.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests.Instructions.Helpers;

namespace UnitTests.Instructions.Data
{
    [TestClass]
    public class TestMovRegReg
        : TestInstructionBase
    {
        public TestMovRegReg()
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

            var program = new []
            {
                new QuickIns(OpCode.MOV_LIT_REG, 
                             new object[] { expected, Registers.R1 }),
                new QuickIns(OpCode.MOV_REG_REG, 
                        new object[] { Registers.R1, Registers.R2 }),
            };

            Vm.Run(Utils.QuickRawCompile(program));

            Assert.IsTrue(Vm.Cpu.Registers[Registers.R2] == expected);
        }
    }
}
