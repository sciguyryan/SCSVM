using VMCore;
using VMCore.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UnitTests.Expressions
{
    public class ExpressionExTestResult
    {
        public string Input;
        public Type ExType;

        public ExpressionExTestResult(string input, Type exType)
        {
            Input = input;
            ExType = exType;
        }

        public override string ToString()
        {
            return $"ExpressionExeptionTestResult({Input}, {ExType})";
        }

        /// <summary>
        /// Run a set of tests for the expression parser.
        /// </summary>
        /// <param name="vm">The virtual machine instance in which the tests should be run.</param>
        /// <param name="tests">An array of the tests to be executed.</param>
        public static void RunTests(VirtualMachine vm,
                                    ExpressionExTestResult[] tests)
        {
            for (var i = 0; i < tests.Length; i++)
            {
                var entry = tests[i];
                bool triggeredException = false;

                try
                {
                    _ = new Parser(entry.Input)
                        .ParseExpression()
                        .Evaluate(vm.CPU);
                }
                catch (Exception ex)
                {
                    if (ex.GetType() != entry.ExType)
                    {
                        Assert.Fail($"Result of test {i} is incorrect. Expected exception of type '{entry.ExType}' to be thrown, however an exception of type '{ex.GetType()}' was thrown instead.");
                    }

                    triggeredException = true;
                }

                if (!triggeredException)
                {
                    Assert.Fail($"Result of test {i} is incorrect. Expected exception of type '{entry.ExType}' to be thrown, however none was thrown.");
                }
            }
        }
    }
}
