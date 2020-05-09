using VMCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Instructions
{
    [TestClass]
    public class Test_SUB_REG_LIT : Test_Instruction_Base
    {
        public Test_SUB_REG_LIT()
        {
        }

        /// <summary>
        /// Test the functionality of a subtraction instruction.
        /// </summary>
        [TestMethod]
        public void TestSubtraction()
        {
            var table = new IntegerTestResult[]
            {
                #region TESTS
                new IntegerTestResult(1, 2, 1, false, false, false),
                new IntegerTestResult(-1, -2, -1, true, false, false), // -2--1 == -2+1 == -1
                new IntegerTestResult(-1, 2, 3, false, false, false), // 2--1 == 2+1 == 3
                new IntegerTestResult(1, -2, -3, true, false, false), // -2-1 == -3
                new IntegerTestResult(0, 0, 0, false, true, false),
                new IntegerTestResult(0, 1, 1, false, false, false),
                new IntegerTestResult(1, 0, -1, true, false, false),

                // These will overflow.
                new IntegerTestResult(-1, int.MaxValue, -2147483648, true, false, true),
                new IntegerTestResult(1, int.MinValue, 2147483647, false, false, true),
                #endregion
            };

            IntegerTestResult.RunTests(_vm, table, OpCode.SUB_REG_LIT);
        }
    }
}
