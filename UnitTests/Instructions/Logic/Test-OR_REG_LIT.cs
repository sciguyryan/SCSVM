using VMCore.VM.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests.Instructions.Helpers;

namespace UnitTests.Instructions.Logic
{
    [TestClass]
    public class TestOrRegLit
        : TestInstructionBase
    {
        public TestOrRegLit()
        {
        }

        /// <summary>
        /// Test the functionality of a logical OR instruction.
        /// </summary>
        [TestMethod]
        public void TestLogicalOr()
        {
            var table = new []
            {
                #region TESTS
                new IntegerTestResult(0, 0, 0, false, true, false),
                new IntegerTestResult(0, 1, 1, false, false, false),
                new IntegerTestResult(1, 0, 1, false, false, false),
                new IntegerTestResult(1, 1, 1, false, false, false),
                new IntegerTestResult(1, -1, -1, true, false, false),
                new IntegerTestResult(-1, 1, -1, true, false, false),
                new IntegerTestResult(-1, -1, -1, true, false, false),
                #endregion
            };

            IntegerTestResult.RunTests(Vm, table, OpCode.OR_REG_LIT);
        }
    }
}