﻿using System;
using System.IO;
using VMCore.Assembler;
using VMCore.VM.Core;
using VMCore.VM.Core.Exceptions;
using VMCore.VM.Core.Register;
using VMCore.VM.Core.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests.Instructions.Helpers;

namespace UnitTests.Instructions.Branching
{
    [TestClass]
    public class TestJgtReg
        : TestInstructionBase
    {
        public TestJgtReg()
        {
        }

        /// <summary>
        /// Test to ensure that jump does occur when
        /// jump condition is met.
        /// </summary>
        [TestMethod]
        public void TestUserAssemblyJumpTakenValid()
        {
            const Registers r1 = Registers.R1;
            const Registers r2 = Registers.R2;
            const Registers r3 = Registers.R3;
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

            var program = new []
            {
                new QuickIns(OpCode.MOV_LIT_REG,
                             new object[] { 100, r1 }),
                new QuickIns(OpCode.SUB_LIT_REG,
                             new object[] { 50, r1 }),
                new QuickIns(OpCode.MOV_LIT_REG,
                             new object[] { 100, r2 }),
                new QuickIns(OpCode.JGT_REG,
                             new object[] { r2, destOffset }),
                new QuickIns(OpCode.HLT),
                new QuickIns(OpCode.MOV_LIT_REG,
                             new object[] { expected, r3 }),
            };

            Vm.Run(Utils.QuickRawCompile(program));

            Assert.IsTrue(Vm.Cpu.Registers[r3] == expected);
        }

        /// <summary>
        /// Test to ensure that jump does not occur when
        /// jump condition is not met.
        /// </summary>
        [TestMethod]
        public void TestUserAssemblyJumpNotTakenValid()
        {
            const Registers r1 = Registers.R1;
            const Registers r2 = Registers.R2;
            const Registers r3 = Registers.R3;
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

            var program = new []
            {
                new QuickIns(OpCode.MOV_LIT_REG,
                             new object[] { 100, r1 }),
                new QuickIns(OpCode.SUB_LIT_REG,
                             new object[] { 50, r1 }),  // AC = 50
                new QuickIns(OpCode.MOV_LIT_REG,
                             new object[] { 50, r2 }),
                new QuickIns(OpCode.JGT_REG,
                             new object[] { r2, destOffset }),
                new QuickIns(OpCode.MOV_LIT_REG,
                             new object[] { expected, r3 }),
                new QuickIns(OpCode.HLT),
                new QuickIns(OpCode.MOV_LIT_REG,
                             new object[] { fail, r3 }),
            };

            Vm.Run(Utils.QuickRawCompile(program));

            Assert.IsTrue(Vm.Cpu.Registers[r3] == expected);
        }


        /// <summary>
        /// Test to ensure that jumping to a valid label
        /// destination works as expected.
        /// </summary>
        [TestMethod]
        public void TestUserAssemblyValidLabel()
        {
            const Registers r1 = Registers.R1;
            const Registers r2 = Registers.R2;
            const Registers r3 = Registers.R3;
            const int expected = 0x123;
            const int fail = 0x321;

            var program = new []
            {
                new QuickIns(OpCode.MOV_LIT_REG,
                             new object[] { 100, r1 }),
                new QuickIns(OpCode.SUB_LIT_REG,
                             new object[] { 50, r1 }),  // AC = 50
                new QuickIns(OpCode.MOV_LIT_REG,
                             new object[] { 100, r2 }),
                new QuickIns(OpCode.JGT_REG,
                             new object[] { r2, 0 },
                             new AsmLabel("GOOD", 1)),
                new QuickIns(OpCode.MOV_LIT_REG,
                             new object[] { fail, r3 }),
                new QuickIns(OpCode.HLT),
                new QuickIns(OpCode.LABEL, new object[] { "GOOD" }),
                new QuickIns(OpCode.MOV_LIT_REG,
                             new object[] { expected, r3 }),
            };

            Vm.Run(Utils.QuickRawCompile(program));

            Assert.IsTrue(Vm.Cpu.Registers[r3] == expected);
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
            const Registers r1 = Registers.R1;
            const Registers r2 = Registers.R2;

            var program = new []
            {
                new QuickIns(OpCode.SUB_LIT_REG,
                             new object[] { 50, r1 }),  // AC = -50
                new QuickIns(OpCode.JGT_REG,
                             new object[] { r2, 0 },
                             new AsmLabel("A", 1)),
            };

            Vm.Run(Utils.QuickRawCompile(program));
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
            const Registers r1 = Registers.R1;
            const Registers r2 = Registers.R2;

            var program = new []
            {
                new QuickIns(OpCode.SUB_LIT_REG,
                             new object[] { 50, r1 }),  // AC = -50
                new QuickIns(OpCode.JGT_REG,
                             new object[] { r2, 0 },
                             new AsmLabel("A", 0)),
                new QuickIns(OpCode.LABEL, new object[] { "A" }),
            };

            Vm.Run(Utils.QuickRawCompile(program));
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
            const Registers r1 = Registers.R1;
            const Registers r2 = Registers.R2;

            var program = new []
            {
                new QuickIns(OpCode.MOV_LIT_REG,
                             new object[] { 100, r1 }),
                new QuickIns(OpCode.SUB_LIT_REG,
                             new object[] { 50, r1 }),  // AC = 50
                new QuickIns(OpCode.MOV_LIT_REG,
                             new object[] { 100, r2 }),
                new QuickIns(OpCode.JGT_REG,
                             new object[] { r2, -2 }),
            };

            Vm.Run(Utils.QuickRawCompile(program));

            Vm.Cpu.FetchExecuteNextInstruction();
        }
    }
}
