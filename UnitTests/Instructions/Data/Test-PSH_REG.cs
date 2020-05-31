using VMCore.Assembler;
using VMCore.VM.Core;
using VMCore.VM.Core.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests.Instructions.Helpers;

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

            var program = new QuickIns[8];
            for (var i = 0; i < 8; i += 2)
            {
                program[i] =
                    new QuickIns(OpCode.MOV_LIT_REG,
                        new object[] { i, (Registers)i });

                program[i + 1] =
                    new QuickIns(OpCode.PSH_REG,
                            new object[] {(Registers)i });
            }

            Vm.Run(Utils.QuickRawCompile(program));

            Assert.IsTrue(Vm.Memory.StackTypes.Count == 4);

            var sp = Vm.Cpu.Registers[Registers.SP];
            Assert.IsTrue(sp == Vm.Memory.StackEnd - 4 * sizeof(int));

            for (var j = 6; j >= 0; j -= 2)
            {
                Assert.IsTrue(Vm.Memory.StackPopInt() == j);
            }
        }
    }
}
