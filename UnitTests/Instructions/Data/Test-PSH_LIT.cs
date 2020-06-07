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
            var program = new CompilerIns[10];
            for (var i = 0; i < 10; i++)
            {
                program[i] =
                    new CompilerIns(OpCode.PSH_LIT,
                            new object[] { i });
            }

            Vm.Run(QuickCompile.RawCompile(program));

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
