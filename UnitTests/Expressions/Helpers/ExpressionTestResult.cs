using System.Diagnostics;
using VMCore.Expressions;
using VMCore.VM;
using VMCore.VM.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Expressions.Helpers
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
                if (entry.RegisterValues != null)
                {
                    var regLen = entry.RegisterValues.Length;
                    for (var j = 0; j < regLen; j++)
                    {
                        aVm.Cpu.Registers[(Registers)j] =
                            entry.RegisterValues[j];
                    }
                }

                var value = 0;
                var success = false;
                try
                {
                    value = 
                        new Parser(entry.Input)
                            .ParseExpression()
                            .Evaluate(aVm.Cpu);

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
                        $"An exception occurred when running " +
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
