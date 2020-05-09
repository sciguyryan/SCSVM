using VMCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Instructions
{
    [TestClass]
    public class Test_AND_REG_LIT : Test_Instruction_Base
    {
        public Test_AND_REG_LIT()
        {
        }

        /// <summary>
        /// Test the functionality of a logical AND instruction.
        /// </summary>
        [TestMethod]
        public void TestLogicalAND()
        {
            var table = new IntegerTestResult[]
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

            IntegerTestResult.RunTests(_vm, table, OpCode.AND_REG_LIT);
        }
    }
}
