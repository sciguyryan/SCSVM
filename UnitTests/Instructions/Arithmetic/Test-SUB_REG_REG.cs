using VMCore.VM.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests.Instructions.Helpers;

namespace UnitTests.Instructions.Arithmetic
{
    [TestClass]
    public class TestSubRegReg
        : TestInstructionBase
    {
        public TestSubRegReg()
        {
        }

        /// <summary>
        /// Test the functionality of a subtraction instruction.
        /// </summary>
        [TestMethod]
        public void TestSubtraction()
        {
            var table = new []
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

            IntegerTestResult.RunTests(Vm, table, OpCode.SUB_REG_REG);
        }
    }
}
