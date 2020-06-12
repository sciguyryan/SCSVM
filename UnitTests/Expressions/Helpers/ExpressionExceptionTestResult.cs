using System;
using System.Collections.Generic;
using VMCore.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Expressions.Helpers
{
    public class ExpressionExTestResult
    {
        public string Input;
        public Type ExType;
        public bool UseVariables;

        public ExpressionExTestResult(string aInput,
                                      Type aExType,
                                      bool aUseVariables = false)
        {
            Input = aInput;
            ExType = aExType;
            UseVariables = aUseVariables;
        }

        public override string ToString()
        {
            return $"ExpressionExceptionTestResult({Input}, {ExType}, {UseVariables})";
        }

        /// <summary>
        /// Run a set of tests for the expression parser.
        /// </summary>
        /// <param name="aTests">
        /// An array of the tests to be executed.
        /// </param>
        public static void RunTests(ExpressionExTestResult[] aTests)
        {
            for (var i = 0; i < aTests.Length; i++)
            {
                var entry = aTests[i];
                var variables = new Dictionary<string, int>();
                if (entry.UseVariables)
                {
                    variables.Add("ABCDEFG", 0);
                }

                var triggeredException = false;

                try
                {
                    _ = new Parser(entry.Input, variables)
                        .ParseExpression()
                        .Evaluate();
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
                        "to be thrown, however none was thrown."
                    );
                }
            }
        }
    }
}
