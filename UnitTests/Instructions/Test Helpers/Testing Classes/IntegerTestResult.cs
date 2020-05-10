using VMCore;
using VMCore.VM;
using VMCore.VM.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Instructions
{
    public class IntegerTestResult
    {
        public int[] Values;
        public int Result;
        public bool SignFlag;
        public bool ZeroFlag;
        public bool OverflowFlag;
        public ResultTypes Type;

        public IntegerTestResult(int value1, int value2, int result, bool signed, bool zero, bool overflow, ResultTypes type = ResultTypes.EQUAL)
        {
            Values = new int[] { value1, value2 };
            Result = result;
            SignFlag = signed;
            ZeroFlag = zero;
            OverflowFlag = overflow;
            Type = type;
        }

        public override string ToString()
        {
            var args = string.Join(",", Values);
            return $"IntegerTest({args}, {Result}, {SignFlag}, {ZeroFlag}, {OverflowFlag})";
        }

        /// <summary>
        /// Run a set of tests within a given virtual machine instance for a given opcode.
        /// </summary>
        /// <param name="vm">The virtual machine instance in which the tests should be run.</param>
        /// <param name="tests">An array of the </param>
        /// <param name="op"></param>
        /// <param name="reg"></param>
        public static void RunTests(VirtualMachine vm, IntegerTestResult[] tests, OpCode op, Registers reg = Registers.AC)
        {
            for (var i = 0; i < tests.Length; i++)
            {
                var entry = tests[i];

                var program = TestUtilties.Generate<int>(op, entry.Values);

                vm.Run(Utils.QuickRawCompile(program));

                bool success = entry.Type switch
                {
                    ResultTypes.EQUAL => vm.CPU.Registers[reg] == entry.Result,
                    _                    => false
                };

                Assert.IsTrue(success,
                              $"Value of register '{reg}' for test {i} is incorrect. Expected {entry.Result}, got {vm.CPU.Registers[reg]}.");

                Assert.IsTrue(vm.CPU.IsFlagSet(CPUFlags.S) == entry.SignFlag,
                              $"Sign flag not correctly set for test {i}. Expected {entry.SignFlag}.");

                Assert.IsTrue(vm.CPU.IsFlagSet(CPUFlags.Z) == entry.ZeroFlag,
                              $"Zero flag not correctly set for test {i}. Expected {entry.ZeroFlag}.");

                Assert.IsTrue(vm.CPU.IsFlagSet(CPUFlags.O) == entry.OverflowFlag,
                              $"Overflow flag not correctly set for test {i}. Expected {entry.OverflowFlag}.");
            }
        }
    }
}
