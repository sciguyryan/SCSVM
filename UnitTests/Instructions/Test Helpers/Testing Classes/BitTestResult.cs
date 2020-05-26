using VMCore;
using VMCore.VM.Core.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Instructions
{
    public class BitTestResult
    {
        public int[] Values;
        public int Result;
        public ResultTypes Type;

        public BitTestResult(int aValue1,
                             int aValue2,
                             int aResult,
                             ResultTypes aType)
        {
            Values = new int[] { aValue1, aValue2 };
            Result = aResult;
            Type = aType;
        }

        public override string ToString()
        {
            return $"BitTest({Values[0]}, {Values[1]}, {Result})";
        }

        /// <summary>
        /// Run a set of tests within a given virtual machine instance for a given opcode.
        /// </summary>
        /// <param name="aVm">The virtual machine instance in which the tests should be run.</param>
        /// <param name="aTests">An array of the tests to be run.</param>
        /// <param name="aOp">The opcode to be tested.</param>
        /// <param name="aReg">The register to be used when checking the result. Defaults to AC.</param>
        public static void RunTests(VirtualMachine aVm,
                                    BitTestResult[] aTests,
                                    OpCode aOp,
                                    Registers aReg = Registers.AC)
        {
            for (var i = 0; i < aTests.Length; i++)
            {
                var entry = aTests[i];

                var program = TestUtilties.Generate<int>(aOp, entry.Values);

                aVm.Run(Utils.QuickRawCompile(program));

                bool success = entry.Type switch
                {
                    ResultTypes.EQUAL => aVm.CPU.Registers[aReg] == entry.Result,
                    _                    => false
                };

                Assert.IsTrue(success,
                              $"Value of register '{aReg}' for test {i} is incorrect. Expected {entry.Result}, got {aVm.CPU.Registers[aReg]}.");
            }
        }
    }
}
