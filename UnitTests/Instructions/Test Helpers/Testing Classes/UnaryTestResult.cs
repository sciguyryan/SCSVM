using System.Collections.Generic;
using VMCore;
using VMCore.Assembler;
using VMCore.VM;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Instructions
{
    public class UnaryTestResult
    {
        public int Value1;
        public int Result;
        public ResultTypes Type;

        public UnaryTestResult(int value1, int result, ResultTypes type)
        {
            Value1 = value1;
            Result = result;
            Type = type;
        }

        public override string ToString()
        {
            return $"UnaryTestResult({Value1}, {Result})";
        }

        public static void RunTests(VirtualMachine vm, UnaryTestResult[] tests, OpCode op)
        {
            var reg = Registers.R1;

            for (var i = 0; i < tests.Length; i++)
            {
                var entry = tests[i];

                var program = new List<QuickIns>
                {
                    new QuickIns(OpCode.MOV_LIT_REG, new object[] { entry.Value1, (byte)reg }),
                    new QuickIns(op, new object[] { (byte)reg }),
                };

                vm.Run(Utils.QuickRawCompile(program));

                bool success = entry.Type switch
                {
                    ResultTypes.EQUAL        => vm.CPU.Registers[reg] == entry.Result,
                    _                           => false
                };

                Assert.IsTrue(success,
                              $"Value of register '{reg}' for test {i} is incorrect. Expected {entry.Result}, got {vm.CPU.Registers[reg]}.");
            }
        }
    }
}
