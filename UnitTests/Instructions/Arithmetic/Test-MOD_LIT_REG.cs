using System;
using VMCore;
using VMCore.Assembler;
using VMCore.VM;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Instructions
{
    [TestClass]
    public class Test_MOD_LIT_REG
        : Test_Instruction_Base
    {
        public Test_MOD_LIT_REG()
        {
        }

        /// <summary>
        /// Test the functionality of a modulo instruction.
        /// </summary>
        [TestMethod]
        public void TestModulo()
        {
            var table = new IntegerTestResult[]
            {
                #region TESTS
                new IntegerTestResult(2, 2, 0, false, true, false),
                new IntegerTestResult(1, 2, 1, false, false, false),
                new IntegerTestResult(2, 1, 0, false, true, false),
                new IntegerTestResult(0, 2, 0, false, true, false),
                new IntegerTestResult(-1, 1, 0, false, true, false),
                new IntegerTestResult(1, -1, 0, false, true, false),
                new IntegerTestResult(-1, -1, 0, false, true, false),
                new IntegerTestResult(-4, -3, -1, true, false, false),
                new IntegerTestResult(-3, -4, -3, true, false, false),
                #endregion
            };

            IntegerTestResult.RunTests(_vm, table, OpCode.MOD_LIT_REG);
        }

        /// <summary>
        /// Test the expected exception behavior of a modulo instruction.
        /// </summary>
        [TestMethod]
        public void TestModuloExceptions()
        {
            var table = new int[][]
            {
                #region TESTS
                new int[] { 0, 0 },
                new int[] { 2, 0 },
                #endregion
            };

            for (var i = 0; i < table.Length; i++)
            {
                var entry = table[i];

                var program = new QuickIns[]
                {
                    new QuickIns(OpCode.MOV_LIT_REG, new object[] { entry[0], (byte)Registers.R1 }),
                    new QuickIns(OpCode.MOD_LIT_REG, new object[] { entry[1], (byte)Registers.R1 }),
                };

                Assert.ThrowsException<DivideByZeroException>(
                    () => _vm.Run(Utils.QuickRawCompile(program)),
                    $"Expected exception of type DivideByZeroException for test {i}.");
            }
        }
    }
}
