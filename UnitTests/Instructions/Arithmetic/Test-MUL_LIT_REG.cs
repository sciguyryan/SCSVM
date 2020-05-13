using VMCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Instructions
{
    [TestClass]
    public class Test_MUL_LIT_REG
        : Test_Instruction_Base
    {
        public Test_MUL_LIT_REG()
        {
        }

        /// <summary>
        /// Test the functionality of a multiply instruction.
        /// </summary>
        [TestMethod]
        public void TestMultiplication()
        {
            var table = new IntegerTestResult[]
            {
                #region TESTS
                new IntegerTestResult(1, 2, 2, false, false, false),
                new IntegerTestResult(-1, -2, 2, false, false, false), // -1 * -2 == 3
                new IntegerTestResult(-1, 2, -2, true, false, false), // -1 * 2 == -2
                new IntegerTestResult(1, -2, -2, true, false, false), // 1 * -2 == -2
                new IntegerTestResult(0, 0, 0, false, true, false),
                new IntegerTestResult(0, 1, 0, false, true, false),
                new IntegerTestResult(1, 0, 0, false, true, false),

                // These will overflow.
                new IntegerTestResult(int.MaxValue, 2, unchecked(int.MaxValue * 2), true, false, true),
                new IntegerTestResult(-1073741825, 2, unchecked(-1073741825 * 2), false, false, true),
                new IntegerTestResult(int.MinValue, 2, 0, false, true, true),
                #endregion
            };

            IntegerTestResult.RunTests(_vm, table, OpCode.MUL_LIT_REG);
        }
    }
}
