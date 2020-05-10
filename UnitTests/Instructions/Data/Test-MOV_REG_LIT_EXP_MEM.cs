using System.Collections.Generic;
using VMCore;
using VMCore.Assembler;
using VMCore.VM;
using VMCore.VM.Core.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Instructions
{
    [TestClass]
    public class MOV_REG_LIT_EXP_MEM
        : Test_Instruction_Base
    {
        public MOV_REG_LIT_EXP_MEM()
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

            var program = new List<QuickIns>
            {
                new QuickIns(OpCode.MOV_LIT_REG, new object[] { expected, r1 }),         // mov $12, R1
                new QuickIns(OpCode.MOV_REG_REG, new object[] { r1, r2 }),               // mov R1, R2
                new QuickIns(OpCode.MOV_REG_LIT_EXP_MEM, new object[] { r1, "R2 + $3" }) // mov R1, [R2 + $3]
            };

            _vm.Run(Utils.QuickRawCompile(program));

            // Extract the value type from memory.
            var intOut = 
                _vm.Memory
                    .GetValueAsType<int>(0x15, SecurityContext.System);

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

            var program = new List<QuickIns>
            {
                new QuickIns(OpCode.MOV_LIT_REG, new object[] { expected, r1 }),            // mov $12, R1
                new QuickIns(OpCode.MOV_LIT_REG, new object[] { -0x5, r2 }),                // mov -$5, R2
                new QuickIns(OpCode.MOV_REG_LIT_EXP_MEM, new object[] { r1, "R2 + $1A" })   // mov R3, [R2 + $1A]
            };

            _vm.Run(Utils.QuickRawCompile(program));

            // Extract the value type from memory.
            var intOut =
                _vm.Memory
                    .GetValueAsType<int>(0x15, SecurityContext.System);

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

            var program = new List<QuickIns>
            {
                new QuickIns(OpCode.MOV_LIT_REG, new object[] { 0, r1 }),                   // mov $0, R1
                new QuickIns(OpCode.MOV_REG_LIT_EXP_MEM, new object[] { r2, "R1 + -$1" })   // mov R2, [R1 + -$1]
            };

            _vm.Run(Utils.QuickRawCompile(program));
        }
    }
}
