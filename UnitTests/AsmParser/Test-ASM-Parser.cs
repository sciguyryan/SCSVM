using System;
using System.Collections.Generic;
using VMCore.AsmParser;
using VMCore.Assembler;
using VMCore.VM.Core;
using VMCore.VM.Core.Register;
using VMCore.VM.Instructions;
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
        public void ValidInstructionRoundTrips()
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
                    "@GOOD",
                },
                new []
                {
                    // The label name will stop being read
                    // at the first non-alphanumeric character.
                    "jne R1, @GOOD-",
                    "@GOOD",
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
                    "mov [$10*$2], R2",
                },
                new []
                {
                    "mov &[$10*$2], R2",
                },
                new []
                {
                    "mov R2, &[$10*$2]",
                },
                new []
                {
                    "mov R2, &[($10*$2)+$1]",
                },

                #endregion // EXPRESSION TESTS

                #region TYPE HINT TESTS

                new []
                {
                    "mov BYTE &R1, R2",
                },
                new []
                {
                    "mov WORD &R1, R2",
                },
                new []
                {
                    "mov DWORD &R1, R2",
                },

                #endregion // TYPE HINT TESTS

                #region TEXT CASE TESTS

                new []
                {
                    "MOV $0b10, R1",
                },
                new []
                {
                    "mOv $0b10, R1",
                },

                #endregion // TEXT CASE TESTS

                #region LINE CONTINUATION TESTS

                new []
                {
                    "mov \\",
                    "$0xFF, \\",
                    "R1",
                },

                #endregion // LINE CONTINUATION TESTS
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
                                    new [] { null, new AsmLabel("GOOD", 1) }),
                    new CompilerIns(OpCode.LABEL,
                                    new object[] { "GOOD" })
                },
                new []
                {
                    new CompilerIns(OpCode.JNE_REG,
                                    new object[] { Registers.R1, 0 },
                                    new [] { null, new AsmLabel("GOOD", 1) }),
                    new CompilerIns(OpCode.LABEL,
                                    new object[] { "GOOD" })
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
                                    new object[] { 0, "GOOD" },
                                    new AsmLabel[2])
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

                #region EXPRESSION TESTS

                new []
                {
                    new CompilerIns(OpCode.MOV_LIT_REG,
                                    new object[] { 20, Registers.R2 })
                },
                new []
                {
                    new CompilerIns(OpCode.MOV_MEM_REG,
                                    new object[] { 20, Registers.R2 })
                },
                new []
                {
                    new CompilerIns(OpCode.MOV_REG_MEM,
                                   new object[] { Registers.R2, 20 })
                },
                new []
                {
                    new CompilerIns(OpCode.MOV_REG_MEM,
                                    new object[] { Registers.R2, 21 })
                },

                #endregion // EXPRESSION TESTS

                #region TYPE HINT TESTS

                new []
                {
                    new CompilerIns(OpCode.MOV_HREG_PTR_REG,
                                    new object[]
                                    {
                                        InstructionSizeHint.BYTE,
                                        Registers.R1,
                                        Registers.R2
                                    })
                },
                new []
                {
                    new CompilerIns(OpCode.MOV_HREG_PTR_REG,
                                    new object[]
                                    {
                                        InstructionSizeHint.WORD,
                                        Registers.R1,
                                        Registers.R2
                                    })
                },
                new []
                {
                    new CompilerIns(OpCode.MOV_HREG_PTR_REG,
                                    new object[]
                                    {
                                        InstructionSizeHint.DWORD,
                                        Registers.R1,
                                        Registers.R2
                                    })
                },

                #endregion // TYPE HINT TESTS

                #region TEXT CASE TESTS

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

                #endregion // TEXT CASE TESTS

                #region LINE CONTINUATION TESTS

                new []
                {
                    new CompilerIns(OpCode.MOV_LIT_REG,
                                    new object[] { 0xFF, Registers.R1 })
                },

                #endregion // LINE CONTINUATION TESTS
            };

            #endregion // RESULTS

            var len = tests.Length;
            for (var i = 0; i < len; i++)
            {
                var test = tests[i];
                var testStr =
                    ".section text" + _nl + string.Join(_nl, test);

                try
                {
                    var p1 =
                        _parser
                            .Parse(testStr)
                            .CodeSectionData
                            .ToArray();

                    var p2 = results[i];

                    Assert.IsTrue(FastArrayEquals(p1, p2));
                }
                catch
                {
                    Assert.Fail
                    (
                        $"Test {i} failed.\r\nFirst line = {test[1]}"
                    );
                }
            }
        }

        [TestMethod]
        public void ValidDirectiveRoundTrips()
        {
            #region TESTS

            var tests = new string[][]
            {
                #region EQU DIRECTIVE TESTS

                new []
                {
                    "strLen	equ	#-str",
                },

                #endregion // EQU DIRECTIVE TESTS

                #region DB DIRECTIVE TESTS

                new []
                {
                    // A string, single quotes.
                    "str db 'Hello, world!'",
                },
                new []
                {
                    // A string, double quotes.
                    "str db \"Hello, world!\"",
                },
                new []
                {
                    // A string plus a byte literal.
                    "str db 'Hello, world!',$0xA",
                },
                new []
                {
                    // A sequence of byte literals.
                    "raw db $0x1,$0x2,$0x3,$0x4,$0x5",
                },

                #endregion // DB DIRECTIVE TESTS

                #region TIMES DIRECTIVE TESTS

                new []
                {
                    "buffer times $5 db $0xA",
                },
                new []
                {
                    "buffer times $5 db 'A'",
                },
                new []
                {
                    "buffer times $5 db 'A',$0x31",
                },

                #endregion // TIMES DIRECTIVE TESTS

                #region TIMES SUB DIRECTIVE TESTS

                new []
                {
                    "buffer db 'A' times $10-#+buffer db $0",
                },

                #endregion // TIMES SUB DIRECTIVE TESTS

                #region LINE CONTINUATION TESTS

                new []
                {
                    "strLen \\",
                    "equ #-str"
                },
                new []
                {
                    // There should be no line continuation here
                    // as the continuation character is within a 
                    // string.
                    "str db 'Hello\\world!'",
                },
                new []
                {
                    // A sequence of byte literals.
                    "raw db $0x1,$0x2,\\" +
                    "$0x3,$0x4,$0x5",
                },

                #endregion // LINE CONTINUATION TESTS
            };

            #endregion // TESTS

            #region RESULTS

            var results = new CompilerDir[][]
            {
                #region EQU DIRECTIVE TESTS

                new []
                {
                    new CompilerDir(DirectiveCodes.EQU,
                                    "strLen",
                                    null,
                                    "#-str",
                                    null,
                                    null), 
                },

                #endregion // EQU DIRECTIVE TESTS

                #region DB DIRECTIVE TESTS

                new []
                {
                    new CompilerDir(DirectiveCodes.DB,
                                    "str",
                                    new byte[] { 72, 101, 108, 108, 111, 44, 32, 119, 111, 114, 108, 100, 33 },
                                    null,
                                    null,
                                    null),
                },
                new []
                {
                    new CompilerDir(DirectiveCodes.DB,
                                    "str",
                                    new byte[] { 72, 101, 108, 108, 111, 44, 32, 119, 111, 114, 108, 100, 33 },
                                    null,
                                    null,
                                    null),
                },
                new []
                {
                    new CompilerDir(DirectiveCodes.DB,
                                    "str",
                                    new byte[] { 72, 101, 108, 108, 111, 44, 32, 119, 111, 114, 108, 100, 33, 10 },
                                    null,
                                    null,
                                    null),
                },
                new []
                {
                    new CompilerDir(DirectiveCodes.DB,
                                    "raw",
                                    new byte[] { 1, 2, 3, 4, 5 },
                                    null,
                                    null,
                                    null),
                },

                #endregion // DB DIRECTIVE TESTS

                #region TIMES DIRECTIVE TESTS

                new []
                {
                    new CompilerDir(DirectiveCodes.DB,
                                    "buffer",
                                    new byte[] { 10 },
                                    null,
                                    "$5",
                                    null),
                },
                new []
                {
                    new CompilerDir(DirectiveCodes.DB,
                                    "buffer",
                                    new byte[] { 65 },
                                    null,
                                    "$5",
                                    null),
                },
                new []
                {
                    new CompilerDir(DirectiveCodes.DB,
                                    "buffer",
                                    new byte[] { 65, 49 },
                                    null,
                                    "$5",
                                    null),
                },

                #endregion // TIMES DIRECTIVE TESTS

                #region TIMES SUB DIRECTIVE TESTS

                new []
                {
                    new CompilerDir(DirectiveCodes.DB,
                                    "buffer",
                                    new byte[] { 65 },
                                    null,
                                    null,
                                    new CompilerDir
                                    (
                                        DirectiveCodes.DB,
                                        "",
                                        new byte[] { 0 },
                                        null,
                                        "$10-#+buffer",
                                        null
                                    )),
                },

                #endregion // TIMES SUB DIRECTIVE TESTS

                #region LINE CONTINUATION TESTS

                new []
                {
                    new CompilerDir(DirectiveCodes.EQU,
                                    "strLen",
                                    null,
                                    "#-str",
                                    null,
                                    null),
                },
                new []
                {
                    new CompilerDir(DirectiveCodes.DB,
                                    "str",
                                    new byte[] { 72, 101, 108, 108, 111, 92, 119, 111, 114, 108, 100, 33 },
                                    null,
                                    null,
                                    null),
                },
                new []
                {
                    new CompilerDir(DirectiveCodes.DB,
                                    "raw",
                                    new byte[] { 1, 2, 3, 4, 5 },
                                    null,
                                    null,
                                    null),
                },

                #endregion // LINE CONTINUATION TESTS
            };

            #endregion // RESULTS

            var len = tests.Length;
            for (var i = 0; i < len; i++)
            {
                var test = tests[i];
                var testStr =
                    ".section data" + _nl + string.Join(_nl, test);

                try
                {
                    var p1 =
                        _parser
                            .Parse(testStr)
                            .DataSectionData
                            .ToArray();

                    var p2 = results[i];

                    Assert.IsTrue(FastArrayEquals(p1, p2));
                }
                catch
                {
                    Assert.Fail
                    (
                        $"Test {i} failed.\r\nFirst line = {test[1]}"
                    );
                }
            }
        }

        [TestMethod]
        public void InvalidInstructionRoundTrips()
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
                var testStr =
                    ".section text" + _nl + string.Join(_nl, test);

                Assert.ThrowsException<AsmParserException>
                (
                    () => _parser.Parse(testStr),
                    "Expected exception of type" +
                           $"ParserException for test {i}. " +
                           $"First line of test = {test[0]}"
                );
            }
        }

        [TestMethod]
        public void InvalidDirectiveRoundTrips()
        {
            #region TESTS

            var tests = new string[][]
            {
                #region INVALID LABEL TESTS

                new []
                {
                    "54 db 'A'",
                },

                #endregion // INVALID LABEL TESTS

                #region INVALID DIRECTIVE TYPE TESTS

                new []
                {
                    "str rr 'A'",
                },

                #endregion INVALID DIRECTIVE TYPE TESTS

                #region INVALID DIRECTIVE ARGUMENT TESTS

                new []
                {
                    "str db",
                },
                new []
                {
                    "str db db",
                },
                new []
                {
                    "str db 54",
                },
                new []
                {
                    "str db 'A",
                },

                #endregion INVALID DIRECTIVE ARGUMENT TESTS

                #region INVALID TIMES DIRECTIVE TESTS

                new []
                {
                    "buffer times",
                },
                new []
                {
                    "buffer times $1 times $0xF",
                },
                new []
                {
                    "buffer db 'A' times db",
                },
                new []
                {
                    // Nested times directives are not supported.
                    "buffer db 'A' times $64-#+buffer times $1",
                },

                #endregion INVALID TIMES DIRECTIVE TESTS
            };

            #endregion // TESTS

            var len = tests.Length;
            for (var i = 0; i < len; i++)
            {
                var test = tests[i];
                var testStr =
                    ".section text" + _nl + string.Join(_nl, test);

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

        private static bool FastArrayEquals(IReadOnlyList<CompilerDir> a1,
                                            IReadOnlyList<CompilerDir> a2)
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
