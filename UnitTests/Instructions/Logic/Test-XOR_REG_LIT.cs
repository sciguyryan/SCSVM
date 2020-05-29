using VMCore.VM.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests.Instructions.Helpers;

namespace UnitTests.Instructions.Logic
{
    [TestClass]
    public class TestXorRegLit
        : TestInstructionBase
    {
        public TestXorRegLit()
        {
        }

        /// <summary>
        /// Test the functionality of a logical XOR instruction.
        /// </summary>
        [TestMethod]
        public void TestLogicalXor()
        {
            var table = new []
            {
                #region TESTS
                new IntegerTestResult(0, 0, 0, false, true, false),
                new IntegerTestResult(0, 1, 1, false, false, false),
                new IntegerTestResult(1, 0, 1, false, false, false),
                new IntegerTestResult(1, 1, 0, false, true, false),

                new IntegerTestResult(1, -1, -2, true, false, false),
                new IntegerTestResult(-1, 1, -2, true, false, false),
                new IntegerTestResult(-1, -1, 0, false, true, false),
                #endregion
            };

            IntegerTestResult.RunTests(Vm, table, OpCode.XOR_REG_LIT);
        }
    }
}
