using VMCore.Assembler;
using VMCore.VM.Core;
using VMCore.VM.Core.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests.Instructions.Helpers;
using VMCore.VM.Core.Register;

namespace UnitTests.Instructions.Data
{
    [TestClass]
    public class TestPop
        : TestInstructionBase
    {
        public TestPop()
        {
        }

        /// <summary>
        /// Test if popping a value from the stack to a register
        /// works as expected.
        /// </summary>
        [TestMethod]
        public void TestPopIntegerValueToRegister()
        {
            Vm.Memory.SetDebuggingEnabled(true);

            var program = new QuickIns[10];

            // Push the values to the stack.
            for (var i = 0; i < 5; i++)
            {
                program[i] =
                    new QuickIns(OpCode.PSH_LIT,
                            new object[] { i + 1 });
            }

            // Push the values to the registers in
            // reverse order so that they align.
            for (int j = 0, r = 4; j < 5; j++, r--)
            {
                program[j + 5] =
                    new QuickIns(OpCode.POP,
                            new object[] { (Registers)r });
            }

            Vm.Run(Utils.QuickRawCompile(program));

            Assert.IsTrue(Vm.Memory.StackTypes.Count == 0);

            var sp = Vm.Cpu.Registers[Registers.SP];
            Assert.IsTrue(sp == Vm.Memory.StackEnd);

            for (var k = 4; k >= 0; k--)
            {
                Assert.IsTrue(Vm.Cpu.Registers[(Registers)k] == k+1);
            }
        }
    }
}
