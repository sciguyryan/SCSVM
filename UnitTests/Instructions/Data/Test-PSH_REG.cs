using VMCore.Assembler;
using VMCore.VM.Core;
using VMCore.VM.Core.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests.Instructions.Helpers;
using VMCore.VM.Core.Register;

namespace UnitTests.Instructions.Data
{
    [TestClass]
    public class TestPshReg
        : TestInstructionBase
    {
        public TestPshReg()
        {
        }

        /// <summary>
        /// Test if pushing register values to the stack works as expected.
        /// </summary>
        [TestMethod]
        public void TestPushRegisterValueToStack()
        {
            Vm.Memory.SetDebuggingEnabled(true);

            var program = new QuickIns[10];

            // Move the values to the registers.
            for (var i = 0; i < 5; i++)
            {
                program[i] =
                    new QuickIns(OpCode.MOV_LIT_REG,
                            new object[] { i + 1, (Registers)i });
            }

            // Push the values to the registers in
            // reverse order so that they align.
            for (int j = 0, r = 4; j < 5; j++, r--)
            {
                program[j + 5] =
                    new QuickIns(OpCode.PSH_REG,
                            new object[] { (Registers)r });
            }

            Vm.Run(Utils.QuickRawCompile(program));

            Assert.IsTrue(Vm.Memory.StackTypes.Count == 5);

            var sp = Vm.Cpu.Registers[(Registers.SP, SystemCtx)];
            Assert.IsTrue(sp == Vm.Memory.StackEnd - (sizeof(int) * 5));

            for (var k = 4; k >= 0; k--)
            {
                Assert.IsTrue(Vm.Cpu.Registers[(Registers)k] == k + 1);
            }
        }
    }
}
