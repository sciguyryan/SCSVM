using VMCore.Assembler;
using VMCore.VM.Core;
using VMCore.VM.Core.Register;
using VMCore.VM.Core.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests.Instructions.Helpers;

namespace UnitTests.Instructions.Data
{
    [TestClass]
    public class TestPshLit
        : TestInstructionBase
    {
        public TestPshLit()
        {
        }

        /// <summary>
        /// Test if pushing literals to the stack works as expected.
        /// </summary>
        [TestMethod]
        public void TestPushLiteralToStack()
        {
            Vm.Memory.SetDebuggingEnabled(true);

            var program = new QuickIns[10];
            for (var i = 0; i < 10; i++)
            {
                program[i] =
                    new QuickIns(OpCode.PSH_LIT,
                            new object[] { i });
            }

            Vm.Run(Utils.QuickRawCompile(program));

            Assert.IsTrue(Vm.Memory.StackTypes.Count == 10);

            var sp = Vm.Cpu.Registers[(Registers.SP, SystemCtx)];
            Assert.IsTrue(sp == Vm.Memory.StackEnd - 10 * sizeof(int));

            for (var j = 9; j >= 0; j--)
            {
                Assert.IsTrue(Vm.Memory.StackPopInt() == j);
            }
        }
    }
}
