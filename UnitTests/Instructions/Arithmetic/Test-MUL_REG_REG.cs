using VMCore.VM.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests.Instructions.Helpers;

namespace UnitTests.Instructions.Arithmetic
{
    [TestClass]
    public class TestMulRegReg
        : TestInstructionBase
    {
        public TestMulRegReg()
        {
        }

        /// <summary>
        /// Test the functionality of a multiply instruction.
        /// </summary>
        [TestMethod]
        public void TestMultiplication()
        {
            var table = new []
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
                new IntegerTestResult(int.MaxValue, 2, -2, true, false, true),
                new IntegerTestResult(-1073741825, 2, 2147483646, false, false, true),
                new IntegerTestResult(int.MinValue, 2, 0, false, true, true),
                #endregion
            };

            IntegerTestResult.RunTests(Vm, table, OpCode.MUL_REG_REG);
        }
    }
}
