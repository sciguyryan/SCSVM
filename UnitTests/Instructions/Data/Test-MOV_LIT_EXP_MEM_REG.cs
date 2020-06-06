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
    public class TestMovLitExpMemReg
        : TestInstructionBase
    {
        public TestMovLitExpMemReg()
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
                new CompilerIns(OpCode.MOV_LIT_REG, 
                        new object[] { expected, r1 }),     // mov $12, R1
                new CompilerIns(OpCode.MOV_REG_MEM, 
                        new object[] { r1, 0x15 }),         // mov R1, [$15]
                new CompilerIns(OpCode.MOV_REG_REG, 
                        new object[] { r1, r2 }),           // mov R1, R2
                new CompilerIns(OpCode.MOV_LIT_EXP_MEM_REG, 
                        new object[] { "R2 + $3", r3 })     // mov [R2 + $3], R3
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
                new CompilerIns(OpCode.MOV_LIT_REG, 
                        new object[] { expected, r1 }),     // mov $12, R1
                new CompilerIns(OpCode.MOV_REG_MEM, 
                        new object[] { r1, 0x15 }),         // mov R1, [$15]
                new CompilerIns(OpCode.MOV_LIT_REG, 
                        new object[] { -0x5, r2 }),         // mov -$5, R2
                new CompilerIns(OpCode.MOV_LIT_EXP_MEM_REG, 
                        new object[] { "R2 + $0x1A", r3 })  // mov [R2 + $1A], R3
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
                new CompilerIns(OpCode.MOV_LIT_REG, 
                        new object[] { 0, r1 }),            // mov $0, R1
                new CompilerIns(OpCode.MOV_LIT_EXP_MEM_REG, 
                        new object[] { "R1 + -$1", r2 })    // mov [R1 + -$1], R2
            };

            Vm.Run(Utils.QuickRawCompile(program));
        }
    }
}
