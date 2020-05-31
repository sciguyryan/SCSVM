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
    public class MovRegLitExpMem
        : TestInstructionBase
    {
        public MovRegLitExpMem()
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

            const int expected = 0x12;

            var program = new []
            {
                new QuickIns(OpCode.MOV_LIT_REG, 
                        new object[] { expected, r1 }),     // mov $12, R1
                new QuickIns(OpCode.MOV_REG_REG, 
                        new object[] { r1, r2 }),           // mov R1, R2
                new QuickIns(OpCode.MOV_REG_LIT_EXP_MEM, 
                        new object[] { r1, "R2 + $3" })     // mov R1, [R2 + $3]
            };

            Vm.Run(Utils.QuickRawCompile(program));

            // Extract the value type from memory.
            var intOut = 
                Vm.Memory
                    .GetInt(0x15, SecurityContext.System, false);

            Assert.IsTrue(intOut == expected);
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

            const int expected = 0x12;

            var program = new []
            {
                new QuickIns(OpCode.MOV_LIT_REG, 
                        new object[] { expected, r1 }),     // mov $12, R1
                new QuickIns(OpCode.MOV_LIT_REG, 
                        new object[] { -0x5, r2 }),         // mov -$5, R2
                new QuickIns(OpCode.MOV_REG_LIT_EXP_MEM, 
                        new object[] { r1, "R2 + $0x1A" })  // mov R3, [R2 + $1A]
            };

            Vm.Run(Utils.QuickRawCompile(program));

            // Extract the value type from memory.
            var intOut =
                Vm.Memory
                    .GetInt(0x15, SecurityContext.System, false);

            Assert.IsTrue(intOut == expected);
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
                             new object[] { 0, r1 }),           // mov $0, R1
                new QuickIns(OpCode.MOV_REG_LIT_EXP_MEM, 
                             new object[] { r2, "R1 + -$1" })   // mov R2, [R1 + -$1]
            };

            Vm.Run(Utils.QuickRawCompile(program));
        }
    }
}
