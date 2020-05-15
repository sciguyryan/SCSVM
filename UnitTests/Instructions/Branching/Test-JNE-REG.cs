using VMCore;
using VMCore.Assembler;
using VMCore.VM;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VMCore.VM.Core.Exceptions;

namespace UnitTests.Instructions
{
    [TestClass]
    public class Test_JNE_REG
        : Test_Instruction_Base
    {
        public Test_JNE_REG()
        {
        }

        /// <summary>
        /// Test JNE with valid destination, jump condition met.
        /// </summary>
        [TestMethod]
        public void TestUserAssemblyJumpTakenValid()
        {
            var r1 = Registers.R1;
            var r2 = Registers.R2;
            var r3 = Registers.R3;
            const int expected = 0x123;

            // This is calculated as follows.
            // sizeof(OpCode) * 5 for the number of
            // instructions to skip.
            // sizeof(int) * 4 for the number of integer
            // arguments to be skipped.
            // sizeof(Registers) * 4 for the number of
            // register arguments to be skipped.
            const int destOffset =
                sizeof(OpCode) * 5 +
                sizeof(int) * 4 +
                sizeof(Registers) * 4;

            var program = new QuickIns[]
            {
                new QuickIns(OpCode.MOV_LIT_REG, new object[] { 100, r1 }),
                new QuickIns(OpCode.SUB_LIT_REG, new object[] { 50, r1 }),
                new QuickIns(OpCode.MOV_LIT_REG, new object[] { 100, r2 }),
                new QuickIns(OpCode.JNE_REG, new object[] { r2, destOffset }),
                new QuickIns(OpCode.HLT),
                new QuickIns(OpCode.MOV_LIT_REG, new object[] { expected, r3 }),
            };

            _vm.Run(Utils.QuickRawCompile(program));

            Assert.IsTrue(_vm.CPU.Registers[r3] == expected);
        }

        /// <summary>
        /// Test JNE with valid destination, jump condition not met.
        /// </summary>
        [TestMethod]
        public void TestUserAssemblyJumpNotTakenValid()
        {
            var r1 = Registers.R1;
            var r2 = Registers.R2;
            var r3 = Registers.R3;
            const int expected = 0x123;
            const int fail = 0x321;

            // This is calculated as follows.
            // sizeof(OpCode) * 6 for the number of
            // instructions to skip.
            // sizeof(int) * 5 for the number of integer
            // arguments to be skipped.
            // sizeof(Registers) * 5 for the number of
            // register arguments to be skipped.
            const int destOffset =
                sizeof(OpCode) * 6 +
                sizeof(int) * 5 +
                sizeof(Registers) * 5;

            var program = new QuickIns[]
            {
                new QuickIns(OpCode.MOV_LIT_REG, new object[] { 100, r1 }),
                new QuickIns(OpCode.SUB_LIT_REG, new object[] { 50, r1 }),  // AC = 50
                new QuickIns(OpCode.MOV_LIT_REG, new object[] { 50, r2 }),
                new QuickIns(OpCode.JNE_REG, new object[] { r2, destOffset }),
                new QuickIns(OpCode.MOV_LIT_REG, new object[] { expected, r3 }),
                new QuickIns(OpCode.HLT),
                new QuickIns(OpCode.MOV_LIT_REG, new object[] { fail, r3 }),
            };

            _vm.Run(Utils.QuickRawCompile(program));

            Assert.IsTrue(_vm.CPU.Registers[r3] == expected);
        }

        /// <summary>
        /// Test JNE with invalid destination. This will crash
        /// as soon as an attempt is made to fetch and execute
        /// the next instruction as the instruction will be read
        /// from non-executable memory.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(MemoryAccessViolationException))]
        public void TestUserAssemblyJumpInvalid()
        {
            var r1 = Registers.R1;
            var r2 = Registers.R2;

            var program = new QuickIns[]
            {
                new QuickIns(OpCode.MOV_LIT_REG, new object[] { 100, r1 }),
                new QuickIns(OpCode.SUB_LIT_REG, new object[] { 50, r1 }),  // AC = 50
                new QuickIns(OpCode.MOV_LIT_REG, new object[] { 100, r2 }),
                new QuickIns(OpCode.JNE_REG, new object[] { r2, -2 }),
            };

            _vm.Run(Utils.QuickRawCompile(program));

            _vm.CPU.FetchExecuteNextInstruction();
        }
    }
}
