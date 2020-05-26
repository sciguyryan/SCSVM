using System;
using System.Collections.Generic;
using VMCore;
using VMCore.Assembler;
using VMCore.VM.Core.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace UnitTests.Instructions
{
    [TestClass]
    public class Test_MOD_REG_LIT
        : Test_Instruction_Base
    {
        public Test_MOD_REG_LIT()
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
                new IntegerTestResult(1, 2, 0, false, true, false),
                new IntegerTestResult(2, 1, 1, false, false, false),
                new IntegerTestResult(2, 0, 0, false, true, false),
                new IntegerTestResult(-1, 1, 0, false, true, false),
                new IntegerTestResult(1, -1, 0, false, true, false),
                new IntegerTestResult(-1, -1, 0, false, true, false),
                new IntegerTestResult(-4, -3, -3, true, false, false),
                new IntegerTestResult(-3, -4, -1, true, false, false),
                #endregion
            };

            IntegerTestResult.RunTests(_vm, table, OpCode.MOD_REG_LIT);
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
                new int[] { 0, 2 },
                #endregion
            };

            for (var i = 0; i < table.Length; i++)
            {
                var entry = table[i];

                var program = new QuickIns[]
                {
                    new QuickIns(OpCode.MOV_LIT_REG, new object[] { entry[0], (byte)Registers.R1 }),
                    new QuickIns(OpCode.MOD_REG_LIT, new object[] { (byte)Registers.R1, entry[1] }),
                };

                Assert.ThrowsException<DivideByZeroException>(
                    () => _vm.Run(Utils.QuickRawCompile(program)),
                    $"Expected exception of type DivideByZeroException for test {i}.");
            }
        }
    }
}
