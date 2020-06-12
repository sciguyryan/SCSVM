using System.Collections.Generic;
using System.Diagnostics;
using VMCore.Expressions;
using VMCore.VM;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Expressions.Helpers
{
    public class ExpressionTestResult
    {
        public string Input;
        public int Result;
        public ResultTypes Type;
        public Dictionary<string, int> Variables;

        public ExpressionTestResult(string aInput,
                                    int aResult,
                                    ResultTypes aType = ResultTypes.EQUAL,
                                    Dictionary<string, int> aVariables = null)
        {
            Input = aInput;
            Result = aResult;
            Type = aType;
            Variables = aVariables ?? new Dictionary<string, int>();
        }

        public override string ToString()
        {
            return $"ExpressionTestResultBasic({Input}, {Result})";
        }

        /// <summary>
        /// Run a set of tests for the expression parser.
        /// </summary>
        /// <param name="aVm">
        /// The virtual machine instance in which the tests should be run.
        /// </param>
        /// <param name="aTests">
        /// An array of the tests to be executed.
        /// </param>
        public static void RunTests(VirtualMachine aVm, 
                                    ExpressionTestResult[] aTests)
        {
            var len = aTests.Length;
            for (var i = 0; i < len; i++)
            {
                var entry = aTests[i];

                var value = 0;
                var success = false;
                try
                {
                    value = 
                        new Parser(entry.Input, entry.Variables)
                            .ParseExpression()
                            .Evaluate();

                    success = entry.Type switch
                    {
                        ResultTypes.EQUAL => value == entry.Result,
                        _                 => false
                    };
                }
                catch
                {
                    Debug.WriteLine
                    (
                        "An exception occurred when running " +
                        $"test {i}. Test string = {entry.Input}."
                    );
                }

                Assert.IsTrue
                (
                    success,
                    $"Result of test {i} is incorrect. " +
                    $"Expected {entry.Result}, got {value}. " +
                    $"Expression = '{entry.Input}'."
                );
            }
        }
    }
}
