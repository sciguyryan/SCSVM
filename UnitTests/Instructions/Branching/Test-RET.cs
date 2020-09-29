using System;
using VMCore.Assembler;
using VMCore.VM.Core;
using VMCore.VM.Core.Register;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests.Instructions.Helpers;

namespace UnitTests.Instructions.Branching
{
    [TestClass]
    public class TestRet
        : TestInstructionBase
    {
        private readonly VMCore.AsmParser.AsmParser _p = 
            new VMCore.AsmParser.AsmParser();

        public TestRet()
        {
        }

        [TestMethod]
        public void TestUserAssemblyReturn()
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

            var pStr = string.Join(Environment.NewLine, lines);

            var program = 
                _p.Parse(pStr).CodeSectionData.ToArray();

            Vm.Run(QuickCompile.RawCompile(program));

            // 0xAAA should be at the top of the stack.
            Assert.IsTrue(Vm.Memory.StackPopInt() == 0xAAA);
            Assert.IsTrue(Vm.Memory.StackPointer == Vm.Memory.StackEnd);

            // Ensure that the execution returns to the correct place
            // after the subroutine returns.
            Assert.IsTrue(Vm.Cpu.Registers[r1] == 0x123);

            // Ensure that the subroutine executed correctly.
            Assert.IsTrue(Vm.Cpu.Registers[ac] == 0x21);
        }

        [TestMethod]
        public void TestUserAssemblyNestedReturn()
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

            var pStr = string.Join(Environment.NewLine, lines);

            var program = 
                _p.Parse(pStr).CodeSectionData.ToArray();

            Vm.Run(QuickCompile.RawCompile(program));

            // 0xAAA should be at the top of the stack.
            Assert.IsTrue(Vm.Memory.StackPopInt() == 0xAAA);
            Assert.IsTrue(Vm.Memory.StackPointer == Vm.Memory.StackEnd);

            // Ensure that the execution returns to the correct place
            // after the subroutine returns.
            Assert.IsTrue(Vm.Cpu.Registers[r1] == 0x123);

            // Ensure that the subroutine executed correctly.
            Assert.IsTrue(Vm.Cpu.Registers[ac] == 0x231);
        }

        /// <summary>
        /// Test that calling a return without a subroutine.
        /// This example should crash however the behavior will
        /// vary depending on the circumstances in which it occurs.
        /// If there are enough entries in the stack to fill the
        /// data expected by the PopState method then calling it will
        /// succeed. That however is undefined behavior and will not
        /// be tested here.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestUserAssemblyInvalidReturn()
        {
            var program = new []
            {
                new CompilerIns(OpCode.RET)
            };

            Vm.Run(QuickCompile.RawCompile(program));
        }
    }
}
