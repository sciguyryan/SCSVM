using VMCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Instructions
{
    [TestClass]
    public class Test_NOT : Test_Instruction_Base
    {
        public Test_NOT()
        {
        }

        /// <summary>
        /// Test the functionality of a logical NOT instruction.
        /// </summary>
        [TestMethod]
        public void TestLogicalNOT()
        {
            var table = new IntegerTestResult[]
            {
                #region TESTS
                new IntegerTestResult(0, 0, -1, true, false, false),
                new IntegerTestResult(-1, 0, 0, false, true, false),
                new IntegerTestResult(1, 0, -2, true, false, false),
                #endregion
            };

            IntegerTestResult.RunTests(_vm, table, OpCode.NOT);
        }
    }
}
