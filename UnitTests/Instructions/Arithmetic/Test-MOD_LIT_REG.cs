using System;
using VMCore.Assembler;
using VMCore.VM.Core;
using VMCore.VM.Core.Register;
using VMCore.VM.Core.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests.Instructions.Helpers;

namespace UnitTests.Instructions.Arithmetic
{
    [TestClass]
    public class TestModLitReg
        : TestInstructionBase
    {
        public TestModLitReg()
        {
        }

        /// <summary>
        /// Test the functionality of a modulo instruction.
        /// </summary>
        [TestMethod]
        public void TestModulo()
        {
            var table = new []
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

            IntegerTestResult.RunTests(Vm, table, OpCode.MOD_LIT_REG);
        }

        /// <summary>
        /// Test the expected exception behavior of a modulo instruction.
        /// </summary>
        [TestMethod]
        public void TestModuloExceptions()
        {
            var table = new []
            {
                new [] { 0, 0 },
                new [] { 2, 0 },
            };

            for (var i = 0; i < table.Length; i++)
            {
                var entry = table[i];

                var program = new []
                {
                    new CompilerIns(OpCode.MOV_LIT_REG, 
                            new object[] { entry[0], (byte)Registers.R1 }),
                    new CompilerIns(OpCode.MOD_LIT_REG, 
                            new object[] { entry[1], (byte)Registers.R1 }),
                };

                Assert.ThrowsException<DivideByZeroException>
                (
                    () => Vm.Run(QuickCompile.RawCompile(program)),
                    $"Expected exception of type DivideByZeroException for test {i}."
                );
            }
        }
    }
}
