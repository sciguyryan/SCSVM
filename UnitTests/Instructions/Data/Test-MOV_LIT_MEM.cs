using VMCore.Assembler;
using VMCore.VM.Core;
using VMCore.VM.Core.Exceptions;
using VMCore.VM.Core.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests.Instructions.Helpers;

namespace UnitTests.Instructions.Data
{
    [TestClass]
    public class TestMovLitMem
        : TestInstructionBase
    {
        public TestMovLitMem()
        {
        }

        /// <summary>
        /// Test copying an integer to a valid memory address.
        /// </summary>
        [TestMethod]
        public void TestCopyRegisterToValidMemory()
        {
            const int expected = 0x123;

            var program = new []
            {
                new CompilerIns(OpCode.MOV_LIT_MEM, 
                             new object[] { expected, 0x0 }),
            };

            Vm.Run(Utils.QuickRawCompile(program));

            // Extract the value type from memory.
            var intOut = 
                Vm.Memory.GetInt(0x0, 
                                  SecurityContext.System, 
                                  false);

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
            var program = new []
            {
                new CompilerIns(OpCode.MOV_LIT_MEM, 
                             new object[] { 0x00, int.MaxValue }),
            };

            Vm.Run(Utils.QuickRawCompile(program));
        }
    }
}
