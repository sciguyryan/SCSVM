using VMCore.VM.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests.Instructions.Helpers;

namespace UnitTests.Instructions.Logic
{
    [TestClass]
    public class TestAndRegLit
        : TestInstructionBase
    {
        public TestAndRegLit()
        {
        }

        /// <summary>
        /// Test the functionality of a logical AND instruction.
        /// </summary>
        [TestMethod]
        public void TestLogicalAnd()
        {
            var table = new []
            {
                #region TESTS
                new IntegerTestResult(0, 0, 0, false, true, false),
                new IntegerTestResult(0, 1, 0, false, true, false),
                new IntegerTestResult(1, 0, 0, false, true, false),
                new IntegerTestResult(1, 1, 1, false, false, false),
                new IntegerTestResult(1, -1, 1, false, false, false),
                new IntegerTestResult(-1, 1, 1, false, false, false),
                new IntegerTestResult(-1, -1, -1, true, false, false),
                #endregion
            };

            IntegerTestResult.RunTests(Vm, table, OpCode.AND_REG_LIT);
        }
    }
}
