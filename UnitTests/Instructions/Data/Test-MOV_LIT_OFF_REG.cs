using VMCore.Assembler;
using VMCore.VM.Core;
using VMCore.VM.Core.Exceptions;
using VMCore.VM.Core.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests.Instructions.Helpers;
using VMCore.VM.Core.Register;

namespace UnitTests.Instructions.Data
{
    [TestClass]
    public class TestMovLitOffReg
        : TestInstructionBase
    {
        public TestMovLitOffReg()
        {
        }

        /// <summary>
        /// Test if indirect reading functions as expected with
        /// a valid positive offset.
        /// </summary>
        [TestMethod]
        public void TestUserAssemblyWriteWithPositiveOffset()
        {
            const Registers r1 = Registers.R1;
            const Registers r2 = Registers.R2;
            const Registers r3 = Registers.R3;

            const int expected = 0x12;

            var program = new []
            {
                new QuickIns(OpCode.MOV_LIT_REG, 
                             new object[] { expected, r1 }),    // mov $12, R1
                new QuickIns(OpCode.MOV_REG_MEM, 
                             new object[] { r1, 0x15 }),        // mov R1, [$15]
                new QuickIns(OpCode.MOV_REG_REG, 
                             new object[] { r1, r2 }),          // mov R1, R2
                new QuickIns(OpCode.MOV_LIT_OFF_REG, 
                             new object[] { 0x3, r2, r3 })      // mov [$3 + R2], R3
            };

            Vm.Run(Utils.QuickRawCompile(program));

            Assert.IsTrue(Vm.Cpu.Registers[r3] == expected);
        }

        /// <summary>
        /// Test if indirect reading functions as expected with
        /// a valid negative offset.
        /// </summary>
        [TestMethod]
        public void TestUserAssemblyWriteWithSignedOffset()
        {
            const Registers r1 = Registers.R1;
            const Registers r2 = Registers.R2;
            const Registers r3 = Registers.R3;

            const int expected = 0x12;

            var program = new []
            {
                new QuickIns(OpCode.MOV_LIT_REG, 
                             new object[] { expected, r1 }),    // mov $12, R1
                new QuickIns(OpCode.MOV_REG_MEM, 
                             new object[] { r1, 0x15 }),        // mov R1, [$15]
                new QuickIns(OpCode.MOV_LIT_REG, 
                             new object[] { -0x5, r2 }),        // mov -$5, R2
                new QuickIns(OpCode.MOV_LIT_OFF_REG, 
                        new object[] { 0x1A, r2, r3 })          // mov [$1A + R2], R3
            };

            Vm.Run(Utils.QuickRawCompile(program));

            Assert.IsTrue(Vm.Cpu.Registers[r3] == expected);
        }

        /// <summary>
        /// Test if indirect reading throws an exception
        /// when dealing with a net signed offset. This
        /// value cannot point to a valid memory location.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(MemoryOutOfRangeException))]
        public void TestUserAssemblyWriteWithNetSignedOffset()
        {
            const Registers r1 = Registers.R1;
            const Registers r2 = Registers.R2;

            var program = new []
            {
                new QuickIns(OpCode.MOV_LIT_REG, 
                        new object[] { 0, r1 }),        // mov $0, R1
                new QuickIns(OpCode.MOV_LIT_OFF_REG, 
                        new object[] { -0x1, r1, r2 })  // mov [-$1 + R1], R2
            };

            Vm.Run(Utils.QuickRawCompile(program));
        }
    }
}
