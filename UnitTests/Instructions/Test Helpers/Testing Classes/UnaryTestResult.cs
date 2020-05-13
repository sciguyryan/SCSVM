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

        public UnaryTestResult(int aValue1,
                               int aResult,
                               ResultTypes aType)
        {
            Value1 = aValue1;
            Result = aResult;
            Type = aType;
        }

        public override string ToString()
        {
            return $"UnaryTestResult({Value1}, {Result})";
        }

        /// <summary>
        /// Run a set of tests within a given virtual machine instance for a given opcode.
        /// </summary>
        /// <param name="aVm">The virtual machine instance in which the tests should be run.</param>
        /// <param name="aTests">An array of the tests to be run.</param>
        /// <param name="aOp">The opcode to be tested.</param>
        public static void RunTests(VirtualMachine aVm,
                                    UnaryTestResult[] aTests,
                                    OpCode aOp)
        {
            var reg = Registers.R1;

            for (var i = 0; i < aTests.Length; i++)
            {
                var entry = aTests[i];

                var program = new QuickIns[]
                {
                    new QuickIns(OpCode.MOV_LIT_REG, new object[] { entry.Value1, (byte)reg }),
                    new QuickIns(aOp, new object[] { (byte)reg }),
                };

                aVm.Run(Utils.QuickRawCompile(program));

                bool success = entry.Type switch
                {
                    ResultTypes.EQUAL   => aVm.CPU.Registers[reg] == entry.Result,
                    _                   => false
                };

                Assert.IsTrue(success,
                              $"Value of register '{reg}' for test {i} is incorrect. Expected {entry.Result}, got {aVm.CPU.Registers[reg]}.");
            }
        }
    }
}
