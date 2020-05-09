using VMCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Instructions
{
    [TestClass]
    public class Test_DEC_REG : Test_Instruction_Base
    {
        public Test_DEC_REG()
        {
        }

        /// <summary>
        /// Test the functionality of a unary decrement instruction.
        /// </summary>
        [TestMethod]
        public void TestDecrement()
        {
            var table = new UnaryTestResult[]
            {
                #region TESTS
                new UnaryTestResult(1, 0, ResultTestType.EQUAL),
                new UnaryTestResult(-1, -2, ResultTestType.EQUAL),
                new UnaryTestResult(2, 1, ResultTestType.EQUAL),
                new UnaryTestResult(0, -1, ResultTestType.EQUAL),

                // This will overflow.
                new UnaryTestResult(int.MinValue, 2147483647, ResultTestType.EQUAL),
                #endregion
            };

            UnaryTestResult.RunTests(_vm, table, OpCode.DEC_REG);
        }
    }
}
