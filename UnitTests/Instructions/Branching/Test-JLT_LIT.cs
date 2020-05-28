using System;
using System.IO;
using VMCore;
using VMCore.Assembler;
using VMCore.VM.Core.Exceptions;
using VMCore.VM.Core.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Instructions
{
    [TestClass]
    public class Test_JLT_LIT
        : Test_Instruction_Base
    {
        public Test_JLT_LIT()
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
            const int expected = 0x123;

            // This is calculated as follows.
            // sizeof(OpCode) * 4 for the number of
            // instructions to skip.
            // sizeof(int) * 4 for the number of integer
            // arguments to be skipped.
            // sizeof(Registers) * 2 for the number of
            // register arguments to be skipped.
            const int destOffset =
                sizeof(OpCode) * 4 +
                sizeof(int) * 4 +
                sizeof(Registers) * 2;

            var program = new QuickIns[]
            {
                new QuickIns(OpCode.MOV_LIT_REG,
                             new object[] { 100, r1 }),
                new QuickIns(OpCode.SUB_LIT_REG,
                             new object[] { 50, r1 }),
                new QuickIns(OpCode.JLT_LIT,
                             new object[] { 10, destOffset }),
                new QuickIns(OpCode.HLT),
                new QuickIns(OpCode.MOV_LIT_REG,
                             new object[] { expected, r2 }),
            };

            _vm.Run(Utils.QuickRawCompile(program));

            Assert.IsTrue(_vm.Cpu.Registers[r2] == expected);
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
            const int expected = 0x123;
            const int fail = 0x321;

            // This is calculated as follows.
            // sizeof(OpCode) * 5 for the number of
            // instructions to skip.
            // sizeof(int) * 5 for the number of integer
            // arguments to be skipped.
            // sizeof(Registers) * 2 for the number of
            // register arguments to be skipped.
            const int destOffset =
                sizeof(OpCode) * 5 +
                sizeof(int) * 5 +
                sizeof(Registers) * 3;

            var program = new QuickIns[]
            {
                new QuickIns(OpCode.MOV_LIT_REG,
                             new object[] { 100, r1 }),
                new QuickIns(OpCode.SUB_LIT_REG,
                             new object[] { 50, r1 }),  // AC = 50,
                new QuickIns(OpCode.JLT_LIT,
                             new object[] { 100, destOffset }),
                new QuickIns(OpCode.MOV_LIT_REG,
                             new object[] { expected, r2 }),
                new QuickIns(OpCode.HLT),
                new QuickIns(OpCode.MOV_LIT_REG,
                             new object[] { fail, r2 }),
            };

            _vm.Run(Utils.QuickRawCompile(program));

            Assert.IsTrue(_vm.Cpu.Registers[r2] == expected);
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
            const int expected = 0x123;
            const int fail = 0x321;

            var program = new QuickIns[]
            {
                new QuickIns(OpCode.MOV_LIT_REG,
                             new object[] { 100, r1 }),
                new QuickIns(OpCode.SUB_LIT_REG,
                             new object[] { 50, r1 }),  // AC = 50
                new QuickIns(OpCode.JLT_LIT,
                             new object[] { 10, 0 },
                             new AsmLabel("GOOD", 1)),
                new QuickIns(OpCode.MOV_LIT_REG,
                             new object[] { fail, r2 }),
                new QuickIns(OpCode.HLT),
                new QuickIns(OpCode.LABEL, new object[] { "GOOD" }),
                new QuickIns(OpCode.MOV_LIT_REG,
                             new object[] { expected, r2 }),
            };

            _vm.Run(Utils.QuickRawCompile(program));

            Assert.IsTrue(_vm.Cpu.Registers[r2] == expected);
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

            var program = new QuickIns[]
            {
                new QuickIns(OpCode.SUB_LIT_REG,
                             new object[] { 50, r1 }),  // AC = -50
                new QuickIns(OpCode.JLT_LIT,
                             new object[] { 0, 0 },
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

            var program = new QuickIns[]
            {
                new QuickIns(OpCode.SUB_LIT_REG,
                             new object[] { 50, r1 }),  // AC = -50
                new QuickIns(OpCode.JLT_LIT,
                             new object[] { 0, 0 },
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

            var program = new QuickIns[]
            {
                new QuickIns(OpCode.MOV_LIT_REG,
                             new object[] { 100, r1 }),
                new QuickIns(OpCode.SUB_LIT_REG,
                             new object[] { 50, r1 }),  // AC = 50
                new QuickIns(OpCode.JLT_LIT,
                             new object[] { 10, -2 }),
            };

            _vm.Run(Utils.QuickRawCompile(program));

            _vm.Cpu.FetchExecuteNextInstruction();
        }
    }
}
