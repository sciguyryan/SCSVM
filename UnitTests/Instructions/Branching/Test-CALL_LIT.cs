﻿using System.IO;
using VMCore.Assembler;
using VMCore.VM.Core;
using VMCore.VM.Core.Register;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests.Instructions.Helpers;

namespace UnitTests.Instructions.Branching
{
    [TestClass]
    public class TestCallLit
        : TestInstructionBase
    {
        public TestCallLit()
        {
        }


        [TestMethod]
        public void TestUserAssemblySubDirectExecution()
        {
            // Ensure that the stack is clean
            // before running each test.
            Vm.Memory.ClearStack();

            const Registers r1 = Registers.R1;
            const Registers ac = Registers.AC;

            // This is calculated as follows.
            // sizeof(OpCode) * 7 for the number of
            // instructions to skip.
            // sizeof(int) * 6 for the number of integer
            // arguments to be skipped.
            // sizeof(Registers) * 1 for the number of
            // register arguments to be skipped.
            const int destOffset =
                sizeof(OpCode) * 7 +
                sizeof(int) * 6 +
                sizeof(Registers) * 1;

            #region Program

            var lines = new[]
            {
                ".section text",
                "push $0xC",    // TESTER Argument 3
                "push $0xB",    // TESTER Argument 2
                "push $0xA",    // TESTER Argument 1
                "push $3",      // The number of arguments for the subroutine
                $"call &${destOffset}",
                "mov $0x123, R1",
                "hlt",

                "TESTER:",
                "mov $0x34, &FP, R3",
                "mov $0x30, &FP, R2",
                "mov $0x2C, &FP, R1",
                "add R1, R2",
                "add R3, AC",
                "ret",
            };

            #endregion // Program

            #region OpCode Order

            var ops = new[]
            {
                OpCode.PSH_LIT,
                OpCode.PSH_LIT,
                OpCode.PSH_LIT,
                OpCode.PSH_LIT,
                OpCode.CAL_LIT,
                OpCode.SUBROUTINE,
                OpCode.MOV_LIT_OFF_REG,
                OpCode.MOV_LIT_OFF_REG,
                OpCode.MOV_LIT_OFF_REG,
                OpCode.ADD_REG_REG,
                OpCode.ADD_REG_REG,
                OpCode.RET,
                OpCode.MOV_LIT_REG,
                OpCode.HLT
            };

            #endregion // OpCode Order

            // Ensure that the execution order for the operations
            // is correct.
            TestUtilities.ExecutionOrderTest(Vm, ops, lines);

            // Ensure that the execution returns to the correct place
            // after the subroutine returns.
            Assert.IsTrue(Vm.Cpu.Registers[r1] == 0x123);

            // Ensure that the subroutine executed correctly.
            Assert.IsTrue(Vm.Cpu.Registers[ac] == 0x21);
        }

        [TestMethod]
        public void TestUserAssemblySubLabelExecution()
        {
            // Ensure that the stack is clean
            // before running each test.
            Vm.Memory.ClearStack();

            const Registers r1 = Registers.R1;
            const Registers ac = Registers.AC;

            #region Program

            var lines = new[]
            {
                ".section text",
                "push $0xAAA",  // Should remain in place once the stack is restored
                "push $0xC",    // TESTER Argument 3
                "push $0xB",    // TESTER Argument 2
                "push $0xA",    // TESTER Argument 1
                "push $3",      // The number of arguments for the subroutine
                "call !TESTER",
                "mov $0x123, R1",
                "hlt",

                "TESTER:",
                "mov $0x34, &FP, R3",
                "mov $0x30, &FP, R2",
                "mov $0x2C, &FP, R1",
                "add R1, R2",
                "add R3, AC",
                "ret",
            };

            #endregion // Program

            #region OpCode Order

            var ops = new []
            {
                OpCode.PSH_LIT,
                OpCode.PSH_LIT,
                OpCode.PSH_LIT,
                OpCode.PSH_LIT,
                OpCode.PSH_LIT,
                OpCode.CAL_LIT,
                OpCode.SUBROUTINE,
                OpCode.MOV_LIT_OFF_REG,
                OpCode.MOV_LIT_OFF_REG,
                OpCode.MOV_LIT_OFF_REG,
                OpCode.ADD_REG_REG,
                OpCode.ADD_REG_REG,
                OpCode.RET,
                OpCode.MOV_LIT_REG,
                OpCode.HLT
            };

            #endregion // OpCode Order

            // Ensure that the execution order for the operations
            // is correct.
            TestUtilities.ExecutionOrderTest(Vm, ops, lines);

            // Ensure that the execution returns to the correct place
            // after the subroutine returns.
            Assert.IsTrue(Vm.Cpu.Registers[r1] == 0x123);

            // Ensure that the subroutine executed correctly.
            Assert.IsTrue(Vm.Cpu.Registers[ac] == 0x21);
        }

        [TestMethod]
        public void TestUserAssemblyNestedSubLabelExecution()
        {
            // Ensure that the stack is clean
            // before running each test.
            Vm.Memory.ClearStack();

            const Registers r1 = Registers.R1;
            const Registers ac = Registers.AC;

            #region Program

            var lines = new[]
            {
                ".section text",
                "push $0xC",    // TESTER Argument 3
                "push $0xB",    // TESTER Argument 2
                "push $0xA",    // TESTER Argument 1
                "push $3",      // The number of arguments for the subroutine
                "call !TESTER",
                "mov $0x123, R1",
                "hlt",

                "TESTER:",
                "mov $0x34, &FP, R3",
                "mov $0x30, &FP, R2",
                "mov $0x2C, &FP, R1",
                "add R1, R2",
                "add R3, AC",
                "push $0xCC",    // TESTER2 Argument 3
                "push $0xBB",    // TESTER2 Argument 2
                "push $0xAA",    // TESTER2 Argument 1
                "push $3",       // The number of arguments for the subroutine
                "call !TESTER2",
                "ret",

                "TESTER2:",
                "mov $0x34, &FP, R3",
                "mov $0x30, &FP, R2",
                "mov $0x2C, &FP, R1",
                "add R1, R2",
                "add R3, AC",
                "ret"
            };

            #endregion // Program

            #region OpCode Order

            var ops = new []
            {
                OpCode.PSH_LIT,
                OpCode.PSH_LIT,
                OpCode.PSH_LIT,
                OpCode.PSH_LIT,
                OpCode.CAL_LIT, // TO TESTER

                OpCode.SUBROUTINE, // [TESTER]
                OpCode.MOV_LIT_OFF_REG,
                OpCode.MOV_LIT_OFF_REG,
                OpCode.MOV_LIT_OFF_REG,
                OpCode.ADD_REG_REG,
                OpCode.ADD_REG_REG,
                OpCode.PSH_LIT,
                OpCode.PSH_LIT,
                OpCode.PSH_LIT,
                OpCode.PSH_LIT,
                OpCode.CAL_LIT,  // TO TESTER2

                OpCode.SUBROUTINE, // [TESTER2]
                OpCode.MOV_LIT_OFF_REG,
                OpCode.MOV_LIT_OFF_REG,
                OpCode.MOV_LIT_OFF_REG,
                OpCode.ADD_REG_REG,
                OpCode.ADD_REG_REG,
                OpCode.RET, // TO TESTER
                OpCode.RET, // TO MAIN

                OpCode.MOV_LIT_REG,
                OpCode.HLT
            };

            #endregion // OpCode Order

            // Ensure that the execution order for the operations
            // is correct.
            TestUtilities.ExecutionOrderTest(Vm, ops, lines);

            // Ensure that the execution returns to the correct place
            // after the subroutine returns.
            Assert.IsTrue(Vm.Cpu.Registers[r1] == 0x123);

            // Ensure that the subroutine executed correctly.
            Assert.IsTrue(Vm.Cpu.Registers[ac] == 0x231);
        }

        [TestMethod]
        public void TestUserAssemblyNestedSubDirectExecution()
        {
            // Ensure that the stack is clean
            // before running each test.
            Vm.Memory.ClearStack();

            // This is calculated as follows.
            // sizeof(OpCode) * 7 for the number of
            // instructions to skip.
            // sizeof(int) * 6 for the number of integer
            // arguments to be skipped.
            // sizeof(Registers) * 1 for the number of
            // register arguments to be skipped.
            const int destOffset1 =
                sizeof(OpCode) * 7 +
                sizeof(int) * 6 +
                sizeof(Registers) * 1;

            // This is calculated as follows.
            // Start with the destination offset
            // as calculated above (for convenience) plus
            // sizeof(OpCode) * 12 for the number of
            // instructions to skip.
            // sizeof(int) * 9 for the number of integer
            // arguments to be skipped. (Don't forget that
            // subroutines have a hidden ID argument!)
            // sizeof(Registers) * 10 for the number of
            // register arguments to be skipped.
            const int destOffset2 =
                destOffset1 +
                sizeof(OpCode) * 12 +
                sizeof(int) * 9 +
                sizeof(Registers) * 10;

            const Registers r1 = Registers.R1;
            const Registers ac = Registers.AC;

            #region Program

            var lines = new[]
            {
                ".section text",
                "push $0xC",    // TESTER Argument 3
                "push $0xB",    // TESTER Argument 2
                "push $0xA",    // TESTER Argument 1
                "push $3",      // The number of arguments for the subroutine
                $"call &${destOffset1}",
                "mov $0x123, R1",
                "hlt",

                "TESTER:",
                "mov $0x34, &FP, R3",
                "mov $0x30, &FP, R2",
                "mov $0x2C, &FP, R1",
                "add R1, R2",
                "add R3, AC",
                "push $0xCC",    // TESTER2 Argument 3
                "push $0xBB",    // TESTER2 Argument 2
                "push $0xAA",    // TESTER2 Argument 1
                "push $3",       // The number of arguments for the subroutine
                $"call &${destOffset2}",
                "ret",

                "TESTER2:",
                "mov $0x34, &FP, R3",
                "mov $0x30, &FP, R2",
                "mov $0x2C, &FP, R1",
                "add R1, R2",
                "add R3, AC",
                "ret"
            };

            #endregion // Program

            #region OpCode Order

            var ops = new[]
            {
                OpCode.PSH_LIT,
                OpCode.PSH_LIT,
                OpCode.PSH_LIT,
                OpCode.PSH_LIT,
                OpCode.CAL_LIT, // TO TESTER

                OpCode.SUBROUTINE, // [TESTER]
                OpCode.MOV_LIT_OFF_REG,
                OpCode.MOV_LIT_OFF_REG,
                OpCode.MOV_LIT_OFF_REG,
                OpCode.ADD_REG_REG,
                OpCode.ADD_REG_REG,
                OpCode.PSH_LIT,
                OpCode.PSH_LIT,
                OpCode.PSH_LIT,
                OpCode.PSH_LIT,
                OpCode.CAL_LIT,  // TO TESTER2

                OpCode.SUBROUTINE, // [TESTER2]
                OpCode.MOV_LIT_OFF_REG,
                OpCode.MOV_LIT_OFF_REG,
                OpCode.MOV_LIT_OFF_REG,
                OpCode.ADD_REG_REG,
                OpCode.ADD_REG_REG,
                OpCode.RET, // TO TESTER
                OpCode.RET, // TO MAIN

                OpCode.MOV_LIT_REG,
                OpCode.HLT
            };

            #endregion // OpCode Order

            // Ensure that the execution order for the operations
            // is correct.
            TestUtilities.ExecutionOrderTest(Vm, ops, lines);

            // Ensure that the execution returns to the correct place
            // after the subroutine returns.
            Assert.IsTrue(Vm.Cpu.Registers[r1] == 0x123);

            // Ensure that the subroutine executed correctly.
            Assert.IsTrue(Vm.Cpu.Registers[ac] == 0x231);
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
            var program = new []
            {
                new CompilerIns(OpCode.CAL_LIT,
                             new object[] { 0 },
                             new AsmLabel("GOOD", 0)),
            };

            Vm.Run(QuickCompile.RawCompile(program));
        }
    }
}
