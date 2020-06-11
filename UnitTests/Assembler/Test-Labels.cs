using System;
using System.IO;
using VMCore.Assembler;
using VMCore.VM;
using VMCore.VM.Core;
using VMCore.VM.Core.Register;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Assembler
{
    [TestClass]
    public class TestLabels
    {
        protected VirtualMachine Vm;

        public TestLabels()
        {
            Vm = new VirtualMachine();
        }

        /// <summary>
        /// Test jump with valid destination after instruction.
        /// </summary>
        [TestMethod]
        public void TestJumpAfterInstruction()
        {
            // We need to add the position at which
            // the program will be loaded in memory.
            // This will give us an absolute
            // address to work with.
            const int address = 
            (
                sizeof(OpCode) * 2 +
                sizeof(Registers) +
                sizeof(int) +
                Compiler.InitialAddress
            );

            var program = new []
            {
                new CompilerIns(OpCode.JNE_REG,
                                new object[] { Registers.R1, 0 },
                                new AsmLabel("A", 1)),
                new CompilerIns(OpCode.NOP),
                new CompilerIns(OpCode.LABEL, new object[] { "A" }),
                new CompilerIns(OpCode.NOP),
            };

            Vm.LoadAndInitialize(QuickCompile.RawCompile(program));

            var ins = Vm.Cpu.Step();
            if (ins is null)
            {
                Assert.Fail();
            }

            Assert.IsTrue(address == (int)ins.Args[1].Value);
        }

        /// <summary>
        /// Test jump with valid destination before instruction.
        /// </summary>
        [TestMethod]
        public void TestJumpBeforeInstruction()
        {
            // We need to add the position at which
            // the program will be loaded in memory.
            // This will give us an absolute
            // address to work with.
            const int address =
            (
                sizeof(OpCode) * 2 +
                sizeof(Registers) +
                sizeof(int) +
                Compiler.InitialAddress
            );

            var program = new []
            {
                new CompilerIns(OpCode.MOV_LIT_REG,
                                new object[] { 0, Registers.R1 }),
                new CompilerIns(OpCode.NOP),
                new CompilerIns(OpCode.LABEL, new object[] { "AAA" }),
                new CompilerIns(OpCode.NOP),

                new CompilerIns(OpCode.JNE_REG,
                                new object[] { Registers.R1, 0 },
                                new [] { new AsmLabel("AAA", 1) }),
            };

            Vm.LoadAndInitialize(QuickCompile.RawCompile(program));

            var ins = new InstructionData()
            {
                OpCode = OpCode.NOP
            };

            var i = 0;
            while (i < 4)
            {
                ins = Vm.Cpu.Step();
                i++;
            }

            if (ins is null)
            {
                Assert.Fail("Expected an instruction but none was returned.");
            }
            
            Assert.IsTrue(address == (int)ins.Args[1].Value);
        }

        /// <summary>
        /// Test to verify that attempting to jump to
        /// an invalid label will throw an exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidDataException))]
        public void TestJumpNoDestination()
        {
            var program = new []
            {
                new CompilerIns(OpCode.JNE_REG,
                                new object[] { Registers.R1, 0 },
                                new AsmLabel("A", 1)),
            };

            Vm.LoadAndInitialize(QuickCompile.RawCompile(program));
        }

        /// <summary>
        /// Test to verify that attempting to jump to add two
        /// labels with the same name will throw an exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidDataException))]
        public void TestDuplicateLabelNames()
        {
            var program = new []
            {
                new CompilerIns(OpCode.LABEL, new object[] { "A" }),
                new CompilerIns(OpCode.LABEL, new object[] { "A" }),
                new CompilerIns(OpCode.NOP),
            };

            Vm.LoadAndInitialize(QuickCompile.RawCompile(program));
        }

        /// <summary>
        /// Test to verify that attempting to bind a label to
        /// an argument that cannot accept it will throw an exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestLabelInvalidArgumentBind()
        {
            var program = new []
            {
                new CompilerIns(OpCode.LABEL, new object[] { "A" }),
                new CompilerIns(OpCode.JNE_REG,
                                new object[] { Registers.R1, 0 },
                                new AsmLabel("A", 0)),
            };

            Vm.LoadAndInitialize(QuickCompile.RawCompile(program));
        }
    }
}
