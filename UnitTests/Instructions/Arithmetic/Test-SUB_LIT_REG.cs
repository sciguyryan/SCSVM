using VMCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Instructions
{
    [TestClass]
    public class Test_SUB_LIT_REG : Test_Instruction_Base
    {
        public Test_SUB_LIT_REG()
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
                new IntegerTestResult(2, 1, 1, false, false, false),
                new IntegerTestResult(-2, -1, -1, true, false, false), // -2--1 == -2+1 == -1
                new IntegerTestResult(2, -1, 3, false, false, false), // 2--1 == 2+1 == 3
                new IntegerTestResult(-2, 1, -3, true, false, false), // -2-1 == -3
                new IntegerTestResult(0, 0, 0, false, true, false),
                new IntegerTestResult(1, 0, 1, false, false, false),
                new IntegerTestResult(0, 1, -1, true, false, false),

                // These will overflow.
                new IntegerTestResult(int.MaxValue, -1, -2147483648, true, false, true),
                new IntegerTestResult(int.MinValue, 1, 2147483647, false, false, true),
                #endregion
            };

            IntegerTestResult.RunTests(_vm, table, OpCode.SUB_LIT_REG);
        }
    }
}
