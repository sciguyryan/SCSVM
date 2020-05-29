using VMCore.VM.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests.Instructions.Helpers;

namespace UnitTests.Instructions.Arithmetic
{
    [TestClass]
    public class TestIncReg
        : TestInstructionBase
    {
        public TestIncReg()
        {
        }

        /// <summary>
        /// Test the functionality of a unary increment instruction.
        /// </summary>
        [TestMethod]
        public void TestIncrement()
        {
            var table = new []
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

            UnaryTestResult.RunTests(Vm, table, OpCode.INC_REG);
        }
    }
}
