using VMCore.VM.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests.Instructions.Helpers;

namespace UnitTests.Instructions.Arithmetic
{
    [TestClass]
    public class TestDecReg
        : TestInstructionBase
    {
        public TestDecReg()
        {
        }

        /// <summary>
        /// Test the functionality of a unary decrement instruction.
        /// </summary>
        [TestMethod]
        public void TestDecrement()
        {
            var table = new []
            {
                #region TESTS
                new UnaryTestResult(1, 0, ResultTypes.EQUAL),
                new UnaryTestResult(-1, -2, ResultTypes.EQUAL),
                new UnaryTestResult(2, 1, ResultTypes.EQUAL),
                new UnaryTestResult(0, -1, ResultTypes.EQUAL),

                // This will overflow.
                new UnaryTestResult(int.MinValue, 2147483647, ResultTypes.EQUAL),
                #endregion
            };

            UnaryTestResult.RunTests(Vm, table, OpCode.DEC_REG);
        }
    }
}
