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
        public ResultTestType Type;

        public ExpressionTestResult(string input,
                                    int result,
                                    int[] registerValues = null,
                                    ResultTestType type = ResultTestType.EQUAL)
        {
            Input = input;
            Result = result;
            RegisterValues = registerValues;
            Type = type;
        }

        public override string ToString()
        {
            return $"ExpressionTestResultBasic({Input}, {Result})";
        }

        /// <summary>
        /// Run a set of tests for the expression parser.
        /// </summary>
        /// <param name="vm">The virtual machine instance in which the tests should be run.</param>
        /// <param name="tests">An array of the tests to be executed.</param>
        public static void RunTests(VirtualMachine vm, 
                                    ExpressionTestResult[] tests)
        {
            for (var i = 0; i < tests.Length; i++)
            {
                var entry = tests[i];
                if (entry.RegisterValues != null)
                {
                    for (var j = 0; j < entry.RegisterValues.Length; j++)
                    {
                        vm.CPU.Registers[(Registers)j] =
                            entry.RegisterValues[j];
                    }
                }

                int value = new Parser(entry.Input)
                    .ParseExpression()
                    .Evaluate(vm.CPU);

                bool success = entry.Type switch
                {
                    ResultTestType.EQUAL    => value == entry.Result,
                    _                       => false
                };

                Assert.IsTrue(success,
                              $"Result of test {i} is incorrect. Expected {entry.Result}, got {value}. Expression = '{entry.Input}'.");
            }
        }
    }
}
