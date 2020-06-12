using System;
using System.Collections.Generic;
using VMCore.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests.Expressions.Helpers;

namespace UnitTests.Expressions
{
    [TestClass]
    public class TestExpressionParser :
        TestExpressionBase
    {
        public TestExpressionParser()
        {
        }

        /// <summary>
        /// Test the functionality of the expression parser.
        /// </summary>
        [TestMethod]
        public void TestExpressionParserBasic()
        {
            var table = new []
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
                                        11)
                ,
                #endregion // SILLY TESTS

                #region VARIABLE TESTS
                new ExpressionTestResult("AAA+BBB-#",
                                         25,
                                         ResultTypes.EQUAL,
                                         new Dictionary<string, int>
                                         {
                                             { "AAA", 10 },
                                             { "BBB", 20 },
                                             { "#", 5 }
                                         }),
                #endregion // VARIABLE TESTS
            };

            ExpressionTestResult.RunTests(Vm, table);
        }

        [TestMethod]
        public void TestExpressionParserExceptions()
        {
            var parserEx = typeof(ExprParserException);

            var table = new []
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
                new ExpressionExTestResult("AA",
                                           typeof(ExprParserException)),

                // This should throw a division by zero
                // exception after the expression is
                // parsed and executed.
                new ExpressionExTestResult("($1-$1)/($1-$1)",
                                           typeof(DivideByZeroException)),

                // These will throw an exception as we are not
                // processing variables here.
                new ExpressionExTestResult("#", parserEx),
                new ExpressionExTestResult("AAA", parserEx),

                // This will fail as the variable is not
                // defined.
                new ExpressionExTestResult("AAA", parserEx, true),

                #endregion
            };

            ExpressionExTestResult.RunTests(table);
        }
    }
}
