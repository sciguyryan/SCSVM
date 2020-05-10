using VMCore;
using VMCore.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Expressions
{
    public class ExpressionTestResult
    {
        public string Input;
        public int Result;
        public int[] RegisterValues;
        public ResultTypes Type;

        public ExpressionTestResult(string aInput,
                                    int aResult,
                                    int[] aRegisterValues = null,
                                    ResultTypes aType = ResultTypes.EQUAL)
        {
            Input = aInput;
            Result = aResult;
            RegisterValues = aRegisterValues;
            Type = aType;
        }

        public override string ToString()
        {
            return $"ExpressionTestResultBasic({Input}, {Result})";
        }

        /// <summary>
        /// Run a set of tests for the expression parser.
        /// </summary>
        /// <param name="aVm">The virtual machine instance in which the tests should be run.</param>
        /// <param name="aTests">An array of the tests to be executed.</param>
        public static void RunTests(VirtualMachine aVm, 
                                    ExpressionTestResult[] aTests)
        {
            for (var i = 0; i < aTests.Length; i++)
            {
                var entry = aTests[i];
                if (entry.RegisterValues != null)
                {
                    for (var j = 0; j < entry.RegisterValues.Length; j++)
                    {
                        aVm.CPU.Registers[(Registers)j] =
                            entry.RegisterValues[j];
                    }
                }

                int value = new Parser(entry.Input)
                    .ParseExpression()
                    .Evaluate(aVm.CPU);

                bool success = entry.Type switch
                {
                    ResultTypes.EQUAL   => value == entry.Result,
                    _                   => false
                };

                Assert.IsTrue(success,
                              $"Result of test {i} is incorrect. Expected {entry.Result}, got {value}. Expression = '{entry.Input}'.");
            }
        }
    }
}
