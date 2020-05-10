using VMCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Instructions
{
    [TestClass]
    public class Test_INC_REG
        : Test_Instruction_Base
    {
        public Test_INC_REG()
        {
        }

        /// <summary>
        /// Test the functionality of a unary increment instruction.
        /// </summary>
        [TestMethod]
        public void TestIncrement()
        {
            var table = new UnaryTestResult[]
            {
                #region TESTS
                new UnaryTestResult(1, 2, ResultTypes.EQUAL),
                new UnaryTestResult(-1, 0, ResultTypes.EQUAL),
                new UnaryTestResult(-2, -1, ResultTypes.EQUAL),
                new UnaryTestResult(0, 1, ResultTypes.EQUAL),

                // This will overflow.
                new UnaryTestResult(int.MaxValue, -2147483648, ResultTypes.EQUAL),
                #endregion
            };

            UnaryTestResult.RunTests(_vm, table, OpCode.INC_REG);
        }
    }
}
