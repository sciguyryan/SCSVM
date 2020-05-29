using VMCore.VM.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests.Instructions.Helpers;

namespace UnitTests.Instructions.Logic
{
    [TestClass]
    public class TestNot
        : TestInstructionBase
    {
        public TestNot()
        {
        }

        /// <summary>
        /// Test the functionality of a logical NOT instruction.
        /// </summary>
        [TestMethod]
        public void TestLogicalNot()
        {
            var table = new []
            {
                #region TESTS
                new IntegerTestResult(0, 0, -1, true, false, false),
                new IntegerTestResult(-1, 0, 0, false, true, false),
                new IntegerTestResult(1, 0, -2, true, false, false),
                #endregion
            };

            IntegerTestResult.RunTests(Vm, table, OpCode.NOT);
        }
    }
}
