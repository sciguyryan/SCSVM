using VMCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Instructions
{
    [TestClass]
    public class Test_ADD_REG_REG
        : Test_Instruction_Base
    {
        public Test_ADD_REG_REG()
        {
        }

        /// <summary>
        /// Test the functionality of an add instruction.
        /// </summary>
        [TestMethod]
        public void TestAddition()
        {
            var table = new IntegerTestResult[]
            {
                #region TESTS
                new IntegerTestResult(1, 2, 3, false, false, false),
                new IntegerTestResult(-1, -2, -3, true, false, false), // -1+-2 == -1-2 == -3
                new IntegerTestResult(-1, 2, 1, false, false, false),
                new IntegerTestResult(1, -2, -1, true, false, false), // 1+-2 == 1 - 2 == -1
                new IntegerTestResult(0, 0, 0, false, true, false),
                new IntegerTestResult(0, 1, 1, false, false, false),
                new IntegerTestResult(1, 0, 1, false, false, false),
                new IntegerTestResult(1, 0, 1, false, false, false),
                new IntegerTestResult(1, -1, 0, false, true, false),

                // These will overflow.
                new IntegerTestResult(int.MaxValue, 1, -2147483648, true, false, true),
                new IntegerTestResult(int.MinValue, -1, 2147483647, false, false, true),
                #endregion
            };

            IntegerTestResult.RunTests(_vm, table, OpCode.ADD_REG_REG);
        }
    }
}
