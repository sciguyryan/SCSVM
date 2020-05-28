using System;
using System.IO;
using VMCore;
using VMCore.Assembler;
using VMCore.VM;
using VMCore.VM.Core;
using VMCore.VM.Core.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Assembler
{
    [TestClass]
    public class Test_Labels
    {
        protected int _mainMemoryCapacity = 2048;
        protected int _stackCapacity = 100;
        protected VirtualMachine _vm;

        public Test_Labels()
        {
            _vm = new VirtualMachine(_mainMemoryCapacity, _stackCapacity);
        }

        /// <summary>
        /// Test jump with valid destination after instruction.
        /// </summary>
        [TestMethod]
        public void TestJumpAfterInstruction()
        {
            const int addr = 
            (
                sizeof(OpCode) * 2 +
                sizeof(Registers) +
                sizeof(int)
            );

            var program = new QuickIns[]
            {
                new QuickIns(OpCode.JNE_REG,
                             new object[] { Registers.R1, 0 },
                             new AsmLabel("A", 1)),
                new QuickIns(OpCode.NOP),
                new QuickIns(OpCode.LABEL, new object[] { "A" }),
                new QuickIns(OpCode.NOP),
            };

            _vm.LoadAndInitialize(Utils.QuickRawCompile(program));

            var ins = _vm.Cpu.Step();

            Assert.IsTrue(addr == (int)ins.Args[1].Value);
        }

        /// <summary>
        /// Test jump with valid destination before instruction.
        /// </summary>
        [TestMethod]
        public void TestJumpBeforeInstruction()
        {
            const int addr =
            (
                sizeof(OpCode) * 2 +
                sizeof(Registers) +
                sizeof(int)
            );

            var program = new QuickIns[]
            {
                new QuickIns(OpCode.MOV_LIT_REG,
                             new object[] { 1, Registers.R1 }),
                new QuickIns(OpCode.NOP),
                new QuickIns(OpCode.LABEL, new object[] { "A" }),
                new QuickIns(OpCode.NOP),

                new QuickIns(OpCode.JNE_REG,
                             new object[] { Registers.R1, 0 },
                             new AsmLabel("A", 1)),
            };

            _vm.LoadAndInitialize(Utils.QuickRawCompile(program));

            InstructionData ins = new InstructionData()
            {
                OpCode = OpCode.NOP
            };

            int i = 0;
            while (i < 4)
            {
                ins = _vm.Cpu.Step();
                i++;
            }

            Assert.IsTrue(addr == (int)ins.Args[1].Value);
        }

        /// <summary>
        /// Test to verify that attempting to jump to
        /// an invalid label will throw an exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidDataException))]
        public void TestJumpNoDestination()
        {
            var program = new QuickIns[]
            {
                new QuickIns(OpCode.JNE_REG,
                             new object[] { Registers.R1, 0 },
                             new AsmLabel("A", 1)),
            };

            _vm.LoadAndInitialize(Utils.QuickRawCompile(program));
        }

        /// <summary>
        /// Test to verify that attempting to jump to add two
        /// labels with the same name will throw an exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidDataException))]
        public void TestDuplicateLabelNames()
        {
            var program = new QuickIns[]
            {
                new QuickIns(OpCode.LABEL, new object[] { "A" }),
                new QuickIns(OpCode.LABEL, new object[] { "A" }),
                new QuickIns(OpCode.NOP),
            };

            _vm.LoadAndInitialize(Utils.QuickRawCompile(program));
        }

        /// <summary>
        /// Test to verify that attempting to bind a label to
        /// an argument that cannot accept it will throw an exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestLabelInvalidArgumentBind()
        {
            var program = new QuickIns[]
            {
                new QuickIns(OpCode.LABEL, new object[] { "A" }),
                new QuickIns(OpCode.JNE_REG,
                             new object[] { Registers.R1, 0 },
                             new AsmLabel("A", 0)),
            };

            _vm.LoadAndInitialize(Utils.QuickRawCompile(program));
        }
    }
}
