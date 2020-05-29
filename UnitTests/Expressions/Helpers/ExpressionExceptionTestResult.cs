using System;
using VMCore.Expressions;
using VMCore.VM;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Expressions.Helpers
{
    public class ExpressionExTestResult
    {
        public string Input;
        public Type ExType;

        public ExpressionExTestResult(string aInput, Type aExType)
        {
            Input = aInput;
            ExType = aExType;
        }

        public override string ToString()
        {
            return $"ExpressionExceptionTestResult({Input}, {ExType})";
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
                                    ExpressionExTestResult[] aTests)
        {
            for (var i = 0; i < aTests.Length; i++)
            {
                var entry = aTests[i];
                bool triggeredException = false;

                try
                {
                    _ = new Parser(entry.Input)
                        .ParseExpression()
                        .Evaluate(aVm.Cpu);
                }
                catch (Exception ex)
                {
                    if (ex.GetType() != entry.ExType)
                    {
                        Assert.Fail
                        (
                            $"Result of test {i} is incorrect. " +
                            "Expected exception of type " +
                            $"'{entry.ExType}' to be thrown, however " +
                            $" an exception of type '{ex.GetType()}' " +
                            "was thrown instead."
                        );
                    }

                    triggeredException = true;
                }

                if (!triggeredException)
                {
                    Assert.Fail
                    (
                        $"Result of test {i} is incorrect. " +
                        $"Expected exception of type '{entry.ExType}' " +
                        $"to be thrown, however none was thrown."
                    );
                }
            }
        }
    }
}
