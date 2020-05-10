using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using VMCore.Expressions;

namespace UnitTests.Expressions
{
    [TestClass]
    public class Test_Expression_Parser :
        Test_Expression_Base
    {
        public Test_Expression_Parser()
        {
        }

        /// <summary>
        /// Test the functionality of the expression parser.
        /// </summary>
        [TestMethod]
        public void TestExpressionParserBasic()
        {
            var table = new ExpressionTestResult[]
            {
                #region BASIC TESTS
                new ExpressionTestResult("+10", 10),
                new ExpressionTestResult("+10", 10),
                new ExpressionTestResult("-10", -10),
                new ExpressionTestResult("10 + 20", 30),
                new ExpressionTestResult("10 - 20", -10),
                new ExpressionTestResult("-10 - 20", -30),
                new ExpressionTestResult("-10 - -20", 10),
                new ExpressionTestResult("10 * 20", 200),
                new ExpressionTestResult("20 / 10", 2),
                new ExpressionTestResult("-10 * 20", -200),
                new ExpressionTestResult("-20 / 10", -2),
                new ExpressionTestResult("-10 * -20", 200),
                new ExpressionTestResult("-20 / -10", 2),
                new ExpressionTestResult("(100 + 20)", 120),
                new ExpressionTestResult("((100) + (20))", 120),
                #endregion

                #region ORDER OF OPERATION TESTS
                new ExpressionTestResult("100 + 10 * 50", 600),
                new ExpressionTestResult("(100 + 10) * 50", 5500),
                new ExpressionTestResult("10 * 50 + 100", 600),
                new ExpressionTestResult("10 * (50 + 100)", 1500),
                new ExpressionTestResult("-100 + 50 / 10", -95),
                new ExpressionTestResult("(-100 + 50) / 10", -5),
                new ExpressionTestResult("-(-100 + 50) / 10", 5),
                new ExpressionTestResult("100 / 2 - 50 + 10 * 20", 200),
                #endregion

                #region HEX TESTS
                new ExpressionTestResult("$10", 16),
                new ExpressionTestResult("$A", 10),
                new ExpressionTestResult("$0A", 10),
                new ExpressionTestResult("$A0", 160),
                new ExpressionTestResult("$AA", 170),
                new ExpressionTestResult("$A + $A", 20),
                new ExpressionTestResult("$A - $A", 0),
                new ExpressionTestResult("-$A - $A", -20),
                new ExpressionTestResult("-$A - -$A", 0),
                #endregion

                #region MIXED BASE
                new ExpressionTestResult("$A + 10", 20),
                new ExpressionTestResult("$A - 10", 0),
                new ExpressionTestResult("-$A - 10", -20),
                new ExpressionTestResult("-$A - -10", 0),
                #endregion

                #region REGISTER TESTS
                new ExpressionTestResult("R1 + 1", 11, new int[] { 10 }),
                new ExpressionTestResult("R1 + $A", 20, new int[] { 10 }),
                new ExpressionTestResult("-R1 + $A", 0, new int[] { 10 }),
                new ExpressionTestResult("R1 + R2", -5, new int[] { 10, -15 }),
                new ExpressionTestResult("R1 * R2", 70, new int[] { 10, 7 }),
                new ExpressionTestResult("R1 / R2 - R3 + R4 * R5", 200, new int[] { 100, 2, 50, 10, 20 }),
                #endregion

                #region SILLY TESTS
                new ExpressionTestResult("+++++++10", 10),
                new ExpressionTestResult("-------10", -10),
                new ExpressionTestResult("+-+-+-+10", -10),
                new ExpressionTestResult("+-+++-+10", 10),
                new ExpressionTestResult("+-+-+-+10+-+++-+10", 0),
                new ExpressionTestResult("+-+-+-+$A+-+++-+10", 0),
                new ExpressionTestResult("((((((5)))))+(((((6))))))", 11),
                #endregion
            };

            ExpressionTestResult.RunTests(_vm, table);
        }

        [TestMethod]
        public void TestExpressionParserExceptions()
        {
            var table = new ExpressionExTestResult[]
            {
                #region TESTS
                new ExpressionExTestResult("(", typeof(ParserException)),
                new ExpressionExTestResult(")", typeof(ParserException)),
                new ExpressionExTestResult("(()", typeof(ParserException)),
                new ExpressionExTestResult("+", typeof(ParserException)),
                new ExpressionExTestResult("+-", typeof(ParserException)),
                new ExpressionExTestResult("$", typeof(ParserException)),
                new ExpressionExTestResult("$X", typeof(ParserException)),
                new ExpressionExTestResult(".", typeof(ParserException)),

                 // This will be treated as a register identifier
                 // and will therefore throw a ArgumentException
                 // when attempting to convert this value
                 // into a register name.
                new ExpressionExTestResult("AA", typeof(ArgumentException)),

                // This should throw a division by zero
                // exception after the expression is
                // parsed and executed.
                new ExpressionExTestResult("(1-1)/(1-1)", typeof(DivideByZeroException)),
                #endregion
            };

            ExpressionExTestResult.RunTests(_vm, table);
        }
    }
}
