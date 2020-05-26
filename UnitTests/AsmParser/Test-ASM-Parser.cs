using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using VMCore;
using VMCore.Assembler;
using VMCore.VM;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.AsmParser
{
    [TestClass]
    public class Test_ASM_Parser
    {
        private readonly VMCore.AsmParser.AsmParser _parser = 
            new VMCore.AsmParser.AsmParser();

        private readonly string _nl = Environment.NewLine;

        public Test_ASM_Parser()
        {
        }

        [TestMethod]
        public void ValidRoundTripTests()
        {
            #region TESTS

            var tests = new string[][]
            {
                #region INTEGER TESTS

                new []
                {
                    "mov $0b10, R1",
                },
                new []
                {
                    "mov $-0b10, R1",
                },
                new []
                {
                    "mov $010, R1",
                },
                new []
                {
                    "mov $-010, R1",
                },
                new []
                {
                    "mov $10, R1",
                },
                new []
                {
                    "mov $-10, R1",
                },
                new []
                {
                    "mov $0x10, R1",
                },
                new []
                {
                    "mov $-0x10, R1",
                },

                #endregion // INTEGER TESTS

                #region LABEL TESTS

                new []
                {
                    "jne R1, @GOOD",
                },
                new []
                {
                    // The label name will stop being read
                    // at the first non-alphanumeric character.
                    "jne R1, @GOOD-",
                },
                new []
                {
                    "@GOOD",
                },
                new []
                {
                    // The label name will stop being read
                    // at the first non-alphanumeric character.
                    "@GOOD-",
                },

                #endregion // LABEL TESTS

                #region REGISTER TESTS

                new []
                {
                    "mov R1, R2",
                },

                #endregion // REGISTER TESTS

                #region REGISTER POINTER TESTS

                new []
                {
                    "mov &R1, R2",
                },
                new []
                {
                    "mov $0x10, &R1, R2",
                },

                #endregion // REGISTER POINTER TESTS

                #region WHITESPACES

                new []
                {
                    "mov                 &R1,\t\t\tR2",
                },
                new []
                {
                    "\t \t mov                 &R1,\t\t\tR2",
                },

                #endregion // WHITESPACES

                #region COMMENTS

                new []
                {
                    "mov $0b10, R1 ; this is a comment",
                },

                #endregion // COMMENTS
            };

            #endregion // TESTS

            #region RESULTS

            var results = new QuickIns[][]
            {
                #region INTEGER TESTS

                new []
                {
                    new QuickIns(OpCode.MOV_LIT_REG,
                                 new object[] { 0b10, Registers.R1 })
                },
                new []
                {
                    new QuickIns(OpCode.MOV_LIT_REG,
                                 new object[] { -0b10, Registers.R1 })
                },
                new []
                {
                    new QuickIns(OpCode.MOV_LIT_REG,
                            new object[] { 8, Registers.R1 })
                },
                new []
                {
                    new QuickIns(OpCode.MOV_LIT_REG,
                                 new object[] { -8, Registers.R1 })
                },
                new []
                {
                    new QuickIns(OpCode.MOV_LIT_REG,
                                 new object[] { 10, Registers.R1 })
                },
                new []
                {
                    new QuickIns(OpCode.MOV_LIT_REG,
                                 new object[] { -10, Registers.R1 })
                },
                new []
                {
                    new QuickIns(OpCode.MOV_LIT_REG,
                                 new object[] { 0x10, Registers.R1 })
                },
                new []
                {
                    new QuickIns(OpCode.MOV_LIT_REG,
                                 new object[] { -0x10, Registers.R1 })
                },

                #endregion // INTEGER TESTS

                #region LABEL TESTS

                new []
                {
                    new QuickIns(OpCode.JNE_REG,
                                 new object[] { Registers.R1, 0 },
                                 new AsmLabel("GOOD", 1))
                },
                new []
                {
                    new QuickIns(OpCode.JNE_REG,
                                 new object[] { Registers.R1, 0 },
                                 new AsmLabel("GOOD", 1))
                },
                new []
                {
                    new QuickIns(OpCode.LABEL,
                                 new object[] { "GOOD" })
                },
                new []
                {
                    new QuickIns(OpCode.LABEL,
                                 new object[] { "GOOD" })
                },

                #endregion // LABEL TESTS

                #region REGISTER TESTS

                new []
                {
                    new QuickIns(OpCode.MOV_REG_REG,
                                 new object[] { Registers.R1, Registers.R2 })
                },

                #endregion // REGISTER TESTS

                #region REGISTER POINTER TESTS

                new []
                {
                    new QuickIns(OpCode.MOV_REG_PTR_REG,
                                new object[] { Registers.R1, Registers.R2 })
                },
                new []
                {
                    new QuickIns(OpCode.MOV_LIT_OFF_REG,
                                 new object[] { 0x10, Registers.R1, Registers.R2 })
                },

                #endregion // REGISTER POINTER TESTS

                #region WHITESPACES

                new []
                {
                    new QuickIns(OpCode.MOV_REG_PTR_REG,
                                 new object[] { Registers.R1, Registers.R2 })
                },
                new []
                {
                    new QuickIns(OpCode.MOV_REG_PTR_REG,
                                 new object[] { Registers.R1, Registers.R2 })
                },

                #endregion // WHITESPACES

                #region COMMENTS

                new []
                {
                    new QuickIns(OpCode.MOV_LIT_REG,
                                 new object[] { 0b10, Registers.R1 })
                },

                #endregion // COMMENTS
            };

            #endregion // RESULTS

            var len = tests.Length;
            for (var i = 0; i < len; i++)
            {
                var test = tests[i];
                var p1 =
                    _parser.Parse(string.Join(_nl, test));

                var p2 = results[i];

                Assert.IsTrue(FastQuickInsArrayEquals(p1, p2),
                              $"Test {i} failed.");
            }
        }

        private static bool FastQuickInsArrayEquals(IReadOnlyList<QuickIns> a1,
                                                    IReadOnlyList<QuickIns> a2)
        {
            if (a1 == null || a2 == null)
            {
                return (a1 == null && a2 == null);
            }

            var len1 = a1.Count;

            if (len1 != a2.Count)
            {
                return false;
            }

            for (var i = 0; i < len1; i++)
            {
                if (a1[i] != a2[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
