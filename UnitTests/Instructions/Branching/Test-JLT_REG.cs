using System;
using System.IO;
using VMCore;
using VMCore.Assembler;
using VMCore.VM;
using VMCore.VM.Core.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Instructions
{
    [TestClass]
    public class Test_JLT_REG
        : Test_Instruction_Base
    {
        public Test_JLT_REG()
        {
        }

        /// <summary>
        /// Test to ensure that jump does occur when
        /// jump condition is met.
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
                new QuickIns(OpCode.MOV_LIT_REG,
                             new object[] { 100, r1 }),
                new QuickIns(OpCode.SUB_LIT_REG,
                             new object[] { 50, r1 }),
                new QuickIns(OpCode.MOV_LIT_REG,
                             new object[] { 10, r2 }),
                new QuickIns(OpCode.JLT_REG,
                             new object[] { r2, destOffset }),
                new QuickIns(OpCode.HLT),
                new QuickIns(OpCode.MOV_LIT_REG,
                             new object[] { expected, r3 }),
            };

            _vm.Run(Utils.QuickRawCompile(program));

            Assert.IsTrue(_vm.CPU.Registers[r3] == expected);
        }

        /// <summary>
        /// Test to ensure that jump does not occur when
        /// jump condition is not met.
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
                new QuickIns(OpCode.MOV_LIT_REG,
                             new object[] { 100, r1 }),
                new QuickIns(OpCode.SUB_LIT_REG,
                             new object[] { 50, r1 }),  // AC = 50
                new QuickIns(OpCode.MOV_LIT_REG,
                             new object[] { 100, r2 }),
                new QuickIns(OpCode.JLT_REG,
                             new object[] { r2, destOffset }),
                new QuickIns(OpCode.MOV_LIT_REG,
                             new object[] { expected, r3 }),
                new QuickIns(OpCode.HLT),
                new QuickIns(OpCode.MOV_LIT_REG,
                             new object[] { fail, r3 }),
            };

            _vm.Run(Utils.QuickRawCompile(program));

            Assert.IsTrue(_vm.CPU.Registers[r3] == expected);
        }


        /// <summary>
        /// Test to ensure that jumping to a valid label
        /// destination works as expected.
        /// </summary>
        [TestMethod]
        public void TestUserAssemblyValidLabel()
        {
            var r1 = Registers.R1;
            var r2 = Registers.R2;
            var r3 = Registers.R3;
            const int expected = 0x123;
            const int fail = 0x321;

            var program = new QuickIns[]
            {
                new QuickIns(OpCode.MOV_LIT_REG,
                             new object[] { 100, r1 }),
                new QuickIns(OpCode.SUB_LIT_REG,
                             new object[] { 50, r1 }),  // AC = 50
                new QuickIns(OpCode.MOV_LIT_REG,
                             new object[] { 10, r2 }),
                new QuickIns(OpCode.JLT_REG,
                             new object[] { r2, 0 },
                             new AsmLabel("GOOD", 1)),
                new QuickIns(OpCode.MOV_LIT_REG,
                             new object[] { fail, r3 }),
                new QuickIns(OpCode.HLT),
                new QuickIns(OpCode.LABEL, new object[] { "GOOD" }),
                new QuickIns(OpCode.MOV_LIT_REG,
                             new object[] { expected, r3 }),
            };

            _vm.Run(Utils.QuickRawCompile(program));

            Assert.IsTrue(_vm.CPU.Registers[r3] == expected);
        }

        /// <summary>
        /// Test jump with invalid label. As the label
        /// name is missing then an exception should 
        /// be thrown at compile time.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidDataException))]
        public void TestUserAssemblyInvalidLabel()
        {
            var r1 = Registers.R1;
            var r2 = Registers.R2;

            var program = new QuickIns[]
            {
                new QuickIns(OpCode.SUB_LIT_REG,
                             new object[] { 50, r1 }),  // AC = -50
                new QuickIns(OpCode.JLT_REG,
                             new object[] { r2, 0 },
                             new AsmLabel("A", 1)),
            };

            _vm.Run(Utils.QuickRawCompile(program));
        }

        /// <summary>
        /// Test binding a label to invalid argument.
        /// This should throw an exception as argument
        /// 0 cannot support label binding.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestUserAssemblyInvalidArgumentBind()
        {
            var r1 = Registers.R1;
            var r2 = Registers.R2;

            var program = new QuickIns[]
            {
                new QuickIns(OpCode.SUB_LIT_REG,
                             new object[] { 50, r1 }),  // AC = -50
                new QuickIns(OpCode.JLT_REG,
                             new object[] { r2, 0 },
                             new AsmLabel("A", 0)),
                new QuickIns(OpCode.LABEL, new object[] { "A" }),
            };

            _vm.Run(Utils.QuickRawCompile(program));
        }

        /// <summary>
        /// Test jump with invalid destination. This will crash
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
                new QuickIns(OpCode.MOV_LIT_REG,
                             new object[] { 100, r1 }),
                new QuickIns(OpCode.SUB_LIT_REG,
                             new object[] { 50, r1 }),  // AC = 50
                new QuickIns(OpCode.MOV_LIT_REG,
                             new object[] { 10, r2 }),
                new QuickIns(OpCode.JLT_REG,
                             new object[] { r2, -2 }),
            };

            _vm.Run(Utils.QuickRawCompile(program));

            _vm.CPU.FetchExecuteNextInstruction();
        }
    }
}
