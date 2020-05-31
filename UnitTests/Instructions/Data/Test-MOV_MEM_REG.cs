using VMCore.Assembler;
using VMCore.VM.Core;
using VMCore.VM.Core.Exceptions;
using VMCore.VM.Core.Register;
using VMCore.VM.Core.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests.Instructions.Helpers;

namespace UnitTests.Instructions.Data
{
    [TestClass]
    public class TestMovMemReg
        : TestInstructionBase
    {
        public TestMovMemReg()
        {
        }

        /// <summary>
        /// Test copying an integer from a valid memory address.
        /// </summary>
        [TestMethod]
        public void TestCopyValidMemoryToRegister()
        {
            const int expected = 0x123;

            var program = new []
            {
                new QuickIns(OpCode.MOV_LIT_MEM, 
                        new object[] { expected, 0x0 }),
                new QuickIns(OpCode.MOV_MEM_REG, 
                        new object[] { 0x0, Registers.R1 }),
            };

            Vm.Run(Utils.QuickRawCompile(program));

            Assert.IsTrue(Vm.Cpu.Registers[Registers.R1] == expected);
        }

        /// <summary>
        /// Test if copying an integer from an invalid memory
        /// address throws an exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(MemoryOutOfRangeException))]
        public void TestCopyInvalidMemoryToRegister()
        {
            var program = new []
            {
                new QuickIns(OpCode.MOV_MEM_REG, 
                        new object[] { int.MaxValue, Registers.R1 }),
            };

            Vm.Run(Utils.QuickRawCompile(program));
        }
    }
}
