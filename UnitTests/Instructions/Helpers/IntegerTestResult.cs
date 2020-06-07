﻿using VMCore.Assembler;
using VMCore.VM;
using VMCore.VM.Core;
using VMCore.VM.Core.Register;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Instructions.Helpers
{
    public class IntegerTestResult
    {
        public int[] Values;
        public int Result;
        public bool SignFlag;
        public bool ZeroFlag;
        public bool OverflowFlag;
        public ResultTypes Type;

        public IntegerTestResult(int aValue1,
                                 int aValue2,
                                 int aResult,
                                 bool aSigned,
                                 bool aZero,
                                 bool aOverflow,
                                 ResultTypes aType = ResultTypes.EQUAL)
        {
            Values = new [] { aValue1, aValue2 };
            Result = aResult;
            SignFlag = aSigned;
            ZeroFlag = aZero;
            OverflowFlag = aOverflow;
            Type = aType;
        }

        public override string ToString()
        {
            var args = string.Join(",", Values);
            return $"IntegerTest({args}, {Result}, " +
                   $"{SignFlag}, {ZeroFlag}, {OverflowFlag})";
        }

        /// <summary>
        /// Run a set of tests within a given virtual machine
        /// instance for a given opcode.
        /// </summary>
        /// <param name="aVm">
        /// The virtual machine instance in which the tests should be run.
        /// </param>
        /// <param name="aTests">An array of the tests to be run.</param>
        /// <param name="aOp">The opcode to be tested.</param>
        /// <param name="aReg">
        /// The register to be used when checking the result.
        /// Defaults to the accumulator (AC).
        /// </param>
        public static void RunTests(VirtualMachine aVm,
                                    IntegerTestResult[] aTests,
                                    OpCode aOp,
                                    Registers aReg = Registers.AC)
        {
            for (var i = 0; i < aTests.Length; i++)
            {
                var entry = aTests[i];

                var program = TestUtilities.Generate(aOp, entry.Values);

                aVm.Run(QuickCompile.RawCompile(program));

                var success = entry.Type switch
                {
                    ResultTypes.EQUAL 
                        => aVm.Cpu.Registers[aReg] == entry.Result,
                    _
                        => false
                };

                Assert.IsTrue(success,
                              $"Value of register '{aReg}' for " +
                              $"test {i} is incorrect. " +
                              $"Expected {entry.Result}, got " +
                              $"{aVm.Cpu.Registers[aReg]}.");

                Assert.IsTrue(aVm.Cpu.IsFlagSet(CpuFlags.S) == entry.SignFlag,
                              "Sign flag not correctly set " +
                              $"for test {i}. " +
                              $"Expected {entry.SignFlag}.");

                Assert.IsTrue(aVm.Cpu.IsFlagSet(CpuFlags.Z) == entry.ZeroFlag,
                              "Zero flag not correctly " +
                              $"set for test {i}. " +
                              $"Expected {entry.ZeroFlag}.");

                Assert.IsTrue(aVm.Cpu.IsFlagSet(CpuFlags.O) == entry.OverflowFlag,
                              $"Overflow flag not correctly set " +
                              $"for test {i}. Expected {entry.OverflowFlag}.");
            }
        }
    }
}
