using VMCore;
using VMCore.VM;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Instructions
{
    public class BitTestResult
    {
        public int[] Values;
        public int Result;
        public ResultTestType Type;

        public BitTestResult(int value1, int value2, int result, ResultTestType type)
        {
            Values = new int[] { value1, value2 };
            Result = result;
            Type = type;
        }

        public override string ToString()
        {
            return $"BitTest({Values[0]}, {Values[1]}, {Result})";
        }

        public static void RunTests(VirtualMachine vm, BitTestResult[] tests, OpCode op, Registers reg = Registers.AC)
        {
            for (var i = 0; i < tests.Length; i++)
            {
                var entry = tests[i];

                var program = TestUtilties.GenerateProgram<int>(op, entry.Values);

                vm.Run(Utils.QuickRawCompile(program));

                bool success = entry.Type switch
                {
                    ResultTestType.EQUAL => vm.CPU.Registers[reg] == entry.Result,
                    _                    => false
                };

                Assert.IsTrue(success,
                              $"Value of register '{reg}' for test {i} is incorrect. Expected {entry.Result}, got {vm.CPU.Registers[reg]}.");
            }
        }
    }
}
