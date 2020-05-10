using VMCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Instructions
{
    [TestClass]
    public class Test_OR_REG_LIT
        : Test_Instruction_Base
    {
        public Test_OR_REG_LIT()
        {
        }

        /// <summary>
        /// Test the functionality of a logical OR instruction.
        /// </summary>
        [TestMethod]
        public void TestLogicalOR()
        {
            var table = new IntegerTestResult[]
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

            IntegerTestResult.RunTests(_vm, table, OpCode.OR_REG_LIT);
        }
    }
}