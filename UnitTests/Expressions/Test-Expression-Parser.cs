using System;
using VMCore.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
                new ExpressionTestResult("+$10", 10),
                new ExpressionTestResult("+$10", 10),
                new ExpressionTestResult("-$10", -10),
                new ExpressionTestResult("$10 + $20", 30),
                new ExpressionTestResult("$10 - $20", -10),
                new ExpressionTestResult("$-10 - $20", -30),
                new ExpressionTestResult("$-10 - $-20", 10),
                new ExpressionTestResult("$10 * $20", 200),
                new ExpressionTestResult("$20 / $10", 2),
                new ExpressionTestResult("$-10 * $20", -200),
                new ExpressionTestResult("$-20 / $10", -2),
                new ExpressionTestResult("$-10 * $-20", 200),
                new ExpressionTestResult("$-20 / $-10", 2),
                new ExpressionTestResult("($100 + $20)", 120),
                new ExpressionTestResult("(($100) + ($20))", 120),
                #endregion // BASIC TESTS

                #region ORDER OF OPERATION TESTS
                new ExpressionTestResult("$100 + $10 * $50",
                                         600),
                new ExpressionTestResult("($100 + $10) * $50",
                                         5500),
                new ExpressionTestResult("$10 * $50 + $100",
                                         600),
                new ExpressionTestResult("$10 * ($50 + $100)",
                                         1500),
                new ExpressionTestResult("$-100 + $50 / $10",
                                         -95),
                new ExpressionTestResult("($-100 + $50) / $10",
                                         -5),
                new ExpressionTestResult("-($-100 + $50) / $10",
                                         5),
                new ExpressionTestResult("$100 / $2 - $50 + $10 * $20",
                                         200),
                #endregion // ORDER OF OPERATION TESTS

                #region HEXADECIMAL TESTS
                new ExpressionTestResult("$0x10", 16),
                new ExpressionTestResult("$0xA", 10),
                new ExpressionTestResult("$0x0A", 10),
                new ExpressionTestResult("$0xA0", 160),
                new ExpressionTestResult("$0xAA", 170),
                new ExpressionTestResult("$0xA + $0xA", 20),
                new ExpressionTestResult("$0xA - $0xA", 0),
                new ExpressionTestResult("$-0xA - $0xA", -20),
                new ExpressionTestResult("$-0xA - -$0xA", 0),
                #endregion // HEXADECIMAL TESTS

                #region OCTAL TESTS
                new ExpressionTestResult("$010", 8),
                new ExpressionTestResult("$-010 - $011", -17),
                new ExpressionTestResult("$-010 - -$010", 0),
                #endregion // OCTAL TESTS

                #region MIXED BASE
                new ExpressionTestResult("$0xA + $10",
                                         20),
                new ExpressionTestResult("$0xA - $10",
                                         0),
                new ExpressionTestResult("$-0xA - $10",
                                         -20),
                new ExpressionTestResult("$-0xA - $-10",
                                         0),
                new ExpressionTestResult("$-0xA - $-10 + $0b11 + $010",
                                         11),
                #endregion // MIXED BASE

                #region REGISTER TESTS
                new ExpressionTestResult("R1 + $1",
                                         11,
                                         new [] { 10 }),
                new ExpressionTestResult("R1 + $0xA",
                                         20,
                                         new [] { 10 }),
                new ExpressionTestResult("-R1 + $0xA",
                                         0,
                                         new [] { 10 }),
                new ExpressionTestResult("R1 + R2",
                                         -5,
                                         new [] { 10, -15 }),
                new ExpressionTestResult("R1 * R2",
                                         70,
                                         new [] { 10, 7 }),
                new ExpressionTestResult("R1 / R2 - R3 + R4 * R5",
                                         200, 
                                         new [] { 100, 2, 50, 10, 20 }),
                #endregion // REGISTER TESTS

                #region SILLY TESTS
                new ExpressionTestResult("+++++++$10",
                                         10),
                new ExpressionTestResult("-------$10",
                                         -10),
                new ExpressionTestResult("+-+-+-+$10",
                                         -10),
                new ExpressionTestResult("+-+++-+$10", 
                                         10),
                new ExpressionTestResult("+-+-+-+$10+-+++-+$10",
                                         0),
                new ExpressionTestResult("+-+-+-+$0xA+-+++-+$10",
                                         0),
                new ExpressionTestResult("(((((($5)))))+((((($6))))))",
                                        11),
                #endregion // SILLY TESTS
            };

            ExpressionTestResult.RunTests(_vm, table);
        }

        [TestMethod]
        public void TestExpressionParserExceptions()
        {
            var parserEx = typeof(ExprParserException);

            var table = new ExpressionExTestResult[]
            {
                #region TESTS
                new ExpressionExTestResult("(", parserEx),
                new ExpressionExTestResult(")", parserEx),
                new ExpressionExTestResult("(()", parserEx),
                new ExpressionExTestResult("+", parserEx),
                new ExpressionExTestResult("+-", parserEx),
                new ExpressionExTestResult("$", parserEx),
                new ExpressionExTestResult("$X", parserEx),
                new ExpressionExTestResult(".", parserEx),
                new ExpressionExTestResult("0xX", parserEx),
                new ExpressionExTestResult("0b2", parserEx),

                // This will be treated as a register identifier
                 // and will therefore throw a ArgumentException
                 // when attempting to convert this value
                 // into a register name.
                new ExpressionExTestResult("AA", typeof(ArgumentException)),

                // This should throw a division by zero
                // exception after the expression is
                // parsed and executed.
                new ExpressionExTestResult("($1-$1)/($1-$1)",
                                           typeof(DivideByZeroException)),
                #endregion
            };

            ExpressionExTestResult.RunTests(_vm, table);
        }
    }
}
