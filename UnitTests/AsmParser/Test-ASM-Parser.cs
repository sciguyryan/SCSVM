using System;
using System.Collections.Generic;
using VMCore.AsmParser;
using VMCore.Assembler;
using VMCore.VM.Core;
using VMCore.VM.Core.Register;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.AsmParser
{
    [TestClass]
    public class TestAsmParser
    {
        private readonly VMCore.AsmParser.AsmParser _parser = 
            new VMCore.AsmParser.AsmParser();

        private readonly string _nl = Environment.NewLine;

        public TestAsmParser()
        {
        }

        [TestMethod]
        public void ValidRoundTripTests()
        {
            #region TESTS

            var tests = new string[][]
            {
                #region INTEGER LITERAL TESTS

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
                new []
                {
                    "mov $0, R1",
                },
                new []
                {
                    "mov $-0, R1",
                },
                new []
                {
                    "mov $-1, R1",
                },
                new []
                {
                    "mov $-01, R1",
                },

                #endregion // INTEGER LITERAL TESTS

                #region INTEGER POINTER TESTS

                new []
                {
                    "mov &$0b10, R1",
                },
                new []
                {
                    "mov &$010, R1",
                },
                new []
                {
                    "mov &$10, R1",
                },
                new []
                {
                    "mov &$0x10, R1",
                },
                new []
                {
                    "mov &$0, R1",
                },

                #endregion // INTEGER POINTER TESTS

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

                #region SUBROUTINE TESTS

                new []
                {
                    "call &$0x123",
                },
                new []
                {
                    "call !GOOD",
                },
                new []
                {
                    // The label name will stop being read
                    // at the first non-alphanumeric character.
                    "call !GOOD-",
                },
                new []
                {
                    "GOOD:",
                },

                #endregion // SUBROUTINE TESTS

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

                #region ARGUMENT TESTS

                new []
                {
                    "hlt",
                },
                new []
                {
                    "mov $0x10, R1",
                },
                new []
                {
                    "mov $0x10, &R1, R2",
                },

                #endregion // ARGUMENT TESTS

                #region EXPRESSION TESTS

                new []
                {
                    "mov [$10+R1], R2",
                },
                new []
                {
                    "mov R2, [$10+R1]",
                },
                new []
                {
                    "mov R2, [($10+R1)+1]",
                },

                #endregion // EXPRESSION TESTS

                #region TEXT CASE

                new []
                {
                    "MOV $0b10, R1",
                },
                new []
                {
                    "mOv $0b10, R1",
                },

                #endregion // TEXT CASE
            };

            #endregion // TESTS

            #region RESULTS

            var results = new CompilerIns[][]
            {
                #region INTEGER LITERAL TESTS

                new []
                {
                    new CompilerIns(OpCode.MOV_LIT_REG,
                                 new object[] { 0b10, Registers.R1 })
                },
                new []
                {
                    new CompilerIns(OpCode.MOV_LIT_REG,
                                 new object[] { -0b10, Registers.R1 })
                },
                new []
                {
                    new CompilerIns(OpCode.MOV_LIT_REG,
                            new object[] { 8, Registers.R1 })
                },
                new []
                {
                    new CompilerIns(OpCode.MOV_LIT_REG,
                                 new object[] { -8, Registers.R1 })
                },
                new []
                {
                    new CompilerIns(OpCode.MOV_LIT_REG,
                                 new object[] { 10, Registers.R1 })
                },
                new []
                {
                    new CompilerIns(OpCode.MOV_LIT_REG,
                                 new object[] { -10, Registers.R1 })
                },
                new []
                {
                    new CompilerIns(OpCode.MOV_LIT_REG,
                                 new object[] { 0x10, Registers.R1 })
                },
                new []
                {
                    new CompilerIns(OpCode.MOV_LIT_REG,
                                 new object[] { -0x10, Registers.R1 })
                },
                new []
                {
                    new CompilerIns(OpCode.MOV_LIT_REG,
                                 new object[] { 0, Registers.R1 })
                },
                new []
                {
                    new CompilerIns(OpCode.MOV_LIT_REG,
                                 new object[] { 0, Registers.R1 })
                },
                new []
                {
                    new CompilerIns(OpCode.MOV_LIT_REG,
                                 new object[] { -1, Registers.R1 })
                },
                new []
                {
                    new CompilerIns(OpCode.MOV_LIT_REG,
                                 new object[] { -1, Registers.R1 })
                },

                #endregion // INTEGER LITERAL TESTS

                #region INTEGER POINTER TESTS

                new []
                {
                    new CompilerIns(OpCode.MOV_MEM_REG,
                                 new object[] { 0b10, Registers.R1 })
                },
                new []
                {
                    new CompilerIns(OpCode.MOV_MEM_REG,
                        new object[] { 8, Registers.R1 })
                },
                new []
                {
                    new CompilerIns(OpCode.MOV_MEM_REG,
                        new object[] { 10, Registers.R1 })
                },
                new []
                {
                    new CompilerIns(OpCode.MOV_MEM_REG,
                        new object[] { 0x10, Registers.R1 })
                },
                new []
                {
                    new CompilerIns(OpCode.MOV_MEM_REG,
                        new object[] { 0, Registers.R1 })
                },

                #endregion // INTEGER POINTER TESTS

                #region LABEL TESTS

                new []
                {
                    new CompilerIns(OpCode.JNE_REG,
                                 new object[] { Registers.R1, 0 },
                                 new AsmLabel("GOOD", 1))
                },
                new []
                {
                    new CompilerIns(OpCode.JNE_REG,
                                 new object[] { Registers.R1, 0 },
                                 new AsmLabel("GOOD", 1))
                },
                new []
                {
                    new CompilerIns(OpCode.LABEL,
                                 new object[] { "GOOD" })
                },
                new []
                {
                    new CompilerIns(OpCode.LABEL,
                                 new object[] { "GOOD" })
                },

                #endregion // LABEL TESTS

                #region SUBROUTINE TESTS

                new []
                {
                    new CompilerIns(OpCode.CAL_LIT,
                                 new object[] { 0x123 })
                },
                new []
                {
                    new CompilerIns(OpCode.CAL_LIT,
                                 new object[] { 0 },
                                 new AsmLabel("GOOD", 0))
                },
                new []
                {
                    new CompilerIns(OpCode.CAL_LIT,
                                 new object[] { 0 },
                                 new AsmLabel("GOOD", 0))
                },
                new []
                {
                    new CompilerIns(OpCode.SUBROUTINE,
                                 new object[] { "GOOD" })
                },

                #endregion // SUBROUTINE TESTS

                #region REGISTER TESTS

                new []
                {
                    new CompilerIns(OpCode.MOV_REG_REG,
                                 new object[] { Registers.R1, Registers.R2 })
                },

                #endregion // REGISTER TESTS

                #region REGISTER POINTER TESTS

                new []
                {
                    new CompilerIns(OpCode.MOV_REG_PTR_REG,
                                new object[] { Registers.R1, Registers.R2 })
                },
                new []
                {
                    new CompilerIns(OpCode.MOV_LIT_OFF_REG,
                                 new object[] { 0x10, Registers.R1, Registers.R2 })
                },

                #endregion // REGISTER POINTER TESTS

                #region WHITESPACES

                new []
                {
                    new CompilerIns(OpCode.MOV_REG_PTR_REG,
                                 new object[] { Registers.R1, Registers.R2 })
                },
                new []
                {
                    new CompilerIns(OpCode.MOV_REG_PTR_REG,
                                 new object[] { Registers.R1, Registers.R2 })
                },

                #endregion // WHITESPACES

                #region COMMENTS

                new []
                {
                    new CompilerIns(OpCode.MOV_LIT_REG,
                                 new object[] { 0b10, Registers.R1 })
                },

                #endregion // COMMENTS

                #region ARGUMENT TESTS

                new []
                {
                    new CompilerIns(OpCode.HLT)
                },
                new []
                {
                    new CompilerIns(OpCode.MOV_LIT_REG,
                                 new object[] { 0x10, Registers.R1 })
                },
                new []
                {
                    new CompilerIns(OpCode.MOV_LIT_OFF_REG,
                        new object[] { 0x10, Registers.R1, Registers.R2 })
                },

                #endregion // ARGUMENT TESTS

                #region TEXT CASE

                new []
                {
                    new CompilerIns(OpCode.MOV_LIT_REG,
                                 new object[] { 0b10, Registers.R1 })
                },
                new []
                {
                    new CompilerIns(OpCode.MOV_LIT_REG,
                                 new object[] { 0b10, Registers.R1 })
                },

                #endregion // TEXT CASE
            };

            #endregion // RESULTS

            var len = tests.Length;
            for (var i = 0; i < len; i++)
            {
                var test = tests[i];
                try
                {
                    var p1 =
                        _parser
                            .Parse(string.Join(_nl, test))
                            .CodeSectionData;

                    var p2 = results[i];

                    Assert.IsTrue(FastArrayEquals(p1, p2),
                                  $"Test {i} failed.");
                }
                catch
                {
                    Assert.Fail
                    (
                        $"Unexpected assertion for test {i}. " +
                        $"First line = {test[0]}"
                    );
                }
            }
        }

        [TestMethod]
        public void InvalidRoundTripTests()
        {
            #region TESTS

            var tests = new string[][]
            {
                #region INVALID INTEGER LITERAL TESTS

                new []
                {
                    "mov $Q, R1",
                },
                new []
                {
                    "mov $, R1",
                },
                new []
                {
                    "mov $-, R1",
                },
                new []
                {
                    "mov $0b, R1",
                },
                new []
                {
                    "mov $0b9, R1",
                },
                new []
                {
                    "mov $0x, R1",
                },
                new []
                {
                    "mov $AA, R1",
                },
                new []
                {
                    "mov $0xXX, R1",
                },
                new []
                {
                    "mov $0A, R1",
                },

                #endregion // INVALID INTEGER LITERAL TESTS

                #region INVALID INTEGER POINTER TESTS

                new []
                {
                    "mov &$-0b10, R1",
                },
                new []
                {
                    "mov &$-010, R1",
                },
                new []
                {
                    "mov &$-10, R1",
                },
                new []
                {
                    "mov &$-0x10, R1",
                },

                #endregion // INVALID INTEGER POINTER TESTS

                #region INVALID REGISTER TESTS

                new []
                {
                    // For some reason Enum.Parse was detecting
                    // the first argument as a valid Registers entry.
                    "mov 53, R8",
                },
                new []
                {
                    "mov $10, R12",
                },
                new []
                {
                    "mov $10, RRR",
                },

                #endregion // INVALID REGISTER TESTS

                #region INVALID REGISTER POINTER TESTS

                new []
                {
                    "mov $10, &R12, R2",
                },
                new []
                {
                    "mov $10, &RRR, R2",
                },

                #endregion // INVALID REGISTER POINTER TESTS

                #region INVALID INSTRUCTION TESTS

                new []
                {
                    // "woof" is an invalid instruction.
                    "woof $10, R12",
                },
                new []
                {
                    // Incorrect number of arguments for an
                    // instruction.
                    "add $10, $10, $10",
                },
                new []
                {
                    // Incorrect parameter types for an instruction.
                    "add &$10, $10",
                },

                #endregion // INVALID INSTRUCTION TESTS

                #region UNMATCHED STRINGS

                new []
                {
                    "lots of random text\"",
                },

                #endregion // UNMATCHED STRINGS

                #region UNMATCHED BRACKETS

                new []
                {
                    "mov [0xAA, R1",
                },
                new []
                {
                    "mov 0xAA], R1",
                },
                new []
                {
                    "mov &R1, [0xAA",
                },
                new []
                {
                    "mov &R1, 0xAA]",
                },

                #endregion // UNMATCHED BRACKETS

                #region INVALID SUBROUTINE LABELS

                new []
                {
                    ":",
                },
                new []
                {
                    "GOOD::",
                },
                new []
                {
                    ":GOOD",
                },
                new []
                {
                    ":GOOD:",
                },
                new []
                {
                    "GOOD-:",
                },

                #endregion // INVALID SUBROUTINE LABELS
            };

            #endregion // TESTS

            var len = tests.Length;
            for (var i = 0; i < len; i++)
            {
                var test = tests[i];

                var testStr = string.Join(_nl, test);

                Assert.ThrowsException<AsmParserException>
                (
                    () => _parser.Parse(testStr),
                    "Expected exception of type" +
                           $"ParserException for test {i}. " +
                           $"First line of test = {test[0]}"
                );
            }
        }

        private static bool FastArrayEquals(IReadOnlyList<CompilerIns> a1,
                                            IReadOnlyList<CompilerIns> a2)
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
