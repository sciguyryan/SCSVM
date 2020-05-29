using VMCore;
using VMCore.Assembler;
using VMCore.VM.Core;
using VMCore.VM.Core.Exceptions;
using VMCore.VM.Core.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests.Instructions.Helpers;

namespace UnitTests.Instructions.Data
{
    [TestClass]
    public class TestMovRegMem
        : TestInstructionBase
    {
        public TestMovRegMem()
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

            var program = new []
            {
                new QuickIns(OpCode.MOV_LIT_REG, 
                             new object[] { expected, Registers.R1 }),
                new QuickIns(OpCode.MOV_REG_MEM, 
                             new object[] { Registers.R1, 0x0 }),
            };

            Vm.Run(Utils.QuickRawCompile(program));

            // Extract the value type from memory.
            var intOut = 
                Vm.Memory
                    .GetInt(0x0, SecurityContext.System, false);

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

            var program = new []
            {
                new QuickIns(OpCode.MOV_LIT_REG, 
                             new object[] { expected, Registers.R1 }),
                new QuickIns(OpCode.MOV_REG_MEM,
                             new object[] { Registers.R1, int.MaxValue }),
            };

            Vm.Run(Utils.QuickRawCompile(program));
        }
    }
}
