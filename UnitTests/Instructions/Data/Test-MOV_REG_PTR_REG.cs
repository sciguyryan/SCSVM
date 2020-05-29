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
    public class TestMovRegPtrReg
        : TestInstructionBase
    {
        public TestMovRegPtrReg()
        {
        }

        /// <summary>
        /// Test indirect reading with valid positive offset.
        /// </summary>
        [TestMethod]
        public void TestUserAssemblyWriteWithPositiveOffset()
        {
            const Registers r1 = Registers.R1;
            const Registers r2 = Registers.R2;
            const Registers r3 = Registers.R3;

            const int address = 0x15;
            const int expected = 0x12;

            var program = new []
            {
                new QuickIns(OpCode.MOV_LIT_REG, 
                             new object[] { expected, r1 }),    // mov $0x12, R1
                new QuickIns(OpCode.MOV_REG_MEM, 
                             new object[] { r1, address }),     // mov R1, $0x15
                new QuickIns(OpCode.MOV_LIT_REG, 
                             new object[] { address, r2 }),     // mov $0x15, R2
                new QuickIns(OpCode.MOV_REG_PTR_REG, 
                             new object[] { r2, r3 })           // mov *R2, R3
            };

            Vm.Run(Utils.QuickRawCompile(program));

            Assert.IsTrue(Vm.Cpu.Registers[r3] == expected);
        }

        /// <summary>
        /// Test indirect reading with invalid negative offset.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(MemoryOutOfRangeException))]
        public void TestUserAssemblyWriteWithNetSignedOffset()
        {
            const Registers r1 = Registers.R1;
            const Registers r2 = Registers.R2;
            const Registers r3 = Registers.R3;

            const int expected = 0x12;

            var program = new []
            {
                new QuickIns(OpCode.MOV_LIT_REG, 
                             new object[] { -0x1, r1 }),    // mov $-0x1, R1
                new QuickIns(OpCode.MOV_REG_PTR_REG, 
                             new object[] { r1, r2 })       // mov *R1, R2
            };


            Vm.Run(Utils.QuickRawCompile(program));

            Assert.IsTrue(Vm.Cpu.Registers[r3] == expected);
        }
    }
}
