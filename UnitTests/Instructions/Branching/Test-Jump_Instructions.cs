using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using VMCore.Assembler;
using VMCore.VM.Core.Exceptions;
using VMCore.VM.Core.Register;
using VMCore.VM.Core.Utilities;
using VMCore.VM.Instructions;
using OpCode = VMCore.VM.Core.OpCode;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests.Instructions.Helpers;

namespace UnitTests.Instructions.Branching
{
    [TestClass]
    public class Test_Jump_Instructions
        : TestInstructionBase
    {
        private enum JumpType
        {
            /// <summary>
            /// Jump if equal.
            /// </summary>
            EQ,
            /// <summary>
            /// Jump if not equal.
            /// </summary>
            NEQ,
            /// <summary>
            /// Jump if less than.
            /// </summary>
            LT,
            /// <summary>
            /// Jump if greater than.
            /// </summary>
            GT,
            /// <summary>
            /// Jump if less than or equal to.
            /// </summary>
            LTE,
            /// <summary>
            /// Jump if greater than or equal to.
            /// </summary>
            GTE,
        }

        private readonly Dictionary<OpCode, JumpType> _jumpTypes;

        /// <summary>
        /// A cached of the opcodes to their instruction instances.
        /// </summary>
        private readonly Dictionary<OpCode, Instruction> _instructionCache =
            ReflectionUtils.InstructionCache;

        public Test_Jump_Instructions()
        {
            _jumpTypes = new Dictionary<OpCode, JumpType>()
            {
                { OpCode.JMP_NOT_EQ, JumpType.NEQ },
                { OpCode.JNE_REG, JumpType.NEQ },
                { OpCode.JEQ_REG, JumpType.EQ },
                { OpCode.JEQ_LIT, JumpType.EQ },
                { OpCode.JLT_REG, JumpType.LT },
                { OpCode.JLT_LIT, JumpType.LT },
                { OpCode.JGT_REG, JumpType.GT },
                { OpCode.JGT_LIT, JumpType.GT },
                { OpCode.JLE_REG, JumpType.LTE },
                { OpCode.JLE_LIT, JumpType.LTE },
                { OpCode.JGE_REG, JumpType.GTE },
                { OpCode.JGE_LIT, JumpType.GTE },
            };
        }

        [TestMethod]
        public void TestJumpTaken()
        {
            foreach (var (op, type) in _jumpTypes)
            {
                RunJumpTestProgram(op, type, false, false);
            }
        }

        [TestMethod]
        public void TestJumpNotTaken()
        {
            foreach (var (op, type) in _jumpTypes)
            {
                RunJumpTestProgram(op, type, true, false);
            }
        }

        [TestMethod]
        public void TestJumpTakenWithLabel()
        {
            foreach (var (op, type) in _jumpTypes)
            {
                RunJumpTestProgram(op, type, false, true);
            }
        }

        [TestMethod]
        public void TestJumpNotTakenWithLabel()
        {
            foreach (var (op, type) in _jumpTypes)
            {
                RunJumpTestProgram(op, type, true, true);
            }
        }

        [TestMethod]
        public void TestJumpInvalidLabelBind()
        {
            foreach (var (op, _) in _jumpTypes)
            {
                var argTypes = _instructionCache[op].ArgumentTypes;
                if (argTypes[0] != typeof(Registers))
                {
                    continue;
                }

                var hadAssert = false;
                try
                {
                    RunInvalidLabelBindProgram(op);
                }
                catch (Exception ex)
                {
                    if (ex.GetType() != typeof(ArgumentException))
                    {
                        Assert.Fail
                        (
                            "Result of label bind test for opcode " +
                            $"{op} was incorrect.\nExpected exception " +
                            $"of type '{typeof(ArgumentException)}' to " +
                            $"be thrown but got {ex.GetType()} instead."
                        );
                    }

                    hadAssert = true;
                }

                if (!hadAssert)
                {
                    Assert.Fail
                    (
                        "Result of label bind test for opcode " +
                        $"{op} was incorrect.\nExpected exception " +
                        $"of type '{typeof(ArgumentException)}' to " +
                        "be thrown, however none was thrown."
                    );
                }
            }
        }

        [TestMethod]
        public void TestJumpInvalidLabel()
        {
            foreach (var (op, _) in _jumpTypes)
            {
                var hadAssert = false;
                try
                {
                    RunInvalidLabelProgram(op);
                }
                catch (Exception ex)
                {
                    if (ex.GetType() != typeof(InvalidDataException))
                    {
                        Assert.Fail
                        (
                            "Result of invalid label test for " +
                            $"opcode '{op}' was incorrect.\n" +
                            "Expected exception of type " +
                            $"'{typeof(ArgumentException)}' to be " +
                            $"thrown but got {ex.GetType()} instead."
                        );
                    }

                    hadAssert = true;
                }

                if (!hadAssert)
                {
                    Assert.Fail
                    (
                        "Result of invalid label test for " +
                        "opcode '{op}' was incorrect." +
                        "\nExpected exception of type " +
                        $"'{typeof(ArgumentException)}' to be thrown, " +
                        "however none was thrown."
                    );
                }
            }
        }

        [TestMethod]
        public void TestJumpInvalidJumpDestination()
        {
            foreach (var (op, type) in _jumpTypes)
            {
                var hadAssert = false;
                try
                {
                    RunInvalidJumpDestinationProgram(op, type);
                }
                catch (Exception ex)
                {
                    if (ex.GetType() != typeof(MemoryAccessViolationException))
                    {
                        Assert.Fail
                        (
                            "Result invalid jump test for opcode " +
                            $"{op} was incorrect.\nExpected exception " +
                            $"of type '{typeof(MemoryAccessViolationException)}' to " +
                            $"be thrown but got {ex.GetType()} instead."
                        );
                    }

                    hadAssert = true;
                }

                if (!hadAssert)
                {
                    Assert.Fail
                    (
                        "Result of invalid jump test for opcode " +
                        $"{op} was incorrect.\nExpected exception " +
                        $"of type '{typeof(MemoryAccessViolationException)}' to " +
                        "be thrown, however none was thrown."
                    );
                }
            }
        }

        private void RunInvalidLabelProgram(OpCode aOp)
        {
            var argTypes = _instructionCache[aOp].ArgumentTypes;

            object arg;
            if (argTypes[0] == typeof(Registers))
            {
                arg = Registers.R1;
            }
            else
            {
                arg = 0;
            }

            var program = new[]
            {
                new CompilerIns(aOp,
                                new [] { arg, 0 },
                                new AsmLabel("A", 1)),
            };

            Vm.Run(QuickCompile.RawCompile(program));
        }


        private void RunInvalidJumpDestinationProgram(OpCode aOp,
                                                      JumpType aType)
        {
            const Registers r1 = Registers.R1;

            // Calculate the value required value to ensure
            // that the jump condition is or isn't taken.
            var value = aType switch
            {
                JumpType.EQ  => 1,
                JumpType.NEQ => 0,
                JumpType.LT  => 1,
                JumpType.GT  => -1,
                JumpType.LTE => 2,
                JumpType.GTE => -2,
                _            => 0
            };

            var program = new List<CompilerIns>();

            object arg;
            var argTypes = _instructionCache[aOp].ArgumentTypes;
            if (argTypes[0] == typeof(Registers))
            {
                // Set the register to the required value.
                program.Add
                (
                    new CompilerIns(OpCode.MOV_LIT_REG,
                        new object[] { value, r1 })
                );

                // This will become the jump instruction
                // condition argument below.
                arg = r1;
            }
            else
            {
                // This will become the jump instruction
                // condition argument below.
                arg = value;
            }

            program.Add(new CompilerIns(aOp,
                                            new [] { arg, -2 }));

            Vm.Run(QuickCompile.RawCompile(program.ToArray()));

            Vm.Cpu.FetchExecuteNextInstruction();
        }

        private void RunInvalidLabelBindProgram(OpCode aOp)
        {
            const Registers r1 = Registers.R1;

            var program = new[]
            {
                new CompilerIns(aOp,
                                new object[] { r1, 0 },
                                new AsmLabel("A", 0)),
                new CompilerIns(OpCode.LABEL, new object[] { "A" }),
            };

            Vm.Run(QuickCompile.RawCompile(program));
        }

        private void RunJumpTestProgram(OpCode aOp,
                                        JumpType aType,
                                        bool aInvert,
                                        bool aUseLabel)
        {
            const Registers ac = Registers.AC;
            const Registers r1 = Registers.R1;
            const Registers r2 = Registers.R2;

            var expected = aInvert ? 0x321 : 0x123;

            var program = new List<CompilerIns>
            {
                // Set the accumulator to be zero.
                // This isn't strictly required as it
                // will default to zero, but just to be
                // safe.
                new CompilerIns(OpCode.MOV_LIT_REG,
                                new object[] {0, ac})
            };

            // Calculate the value required value to ensure
            // that the jump condition is or isn't taken.
            var value = aType switch
            {
                JumpType.EQ  => aInvert ? 1 : 0,
                JumpType.NEQ => aInvert ? 0 : 1,
                JumpType.LT  => aInvert ? 1 : -1,
                JumpType.GT  => aInvert ? -1 : 1,
                JumpType.LTE => aInvert  ? 2 : -2,
                JumpType.GTE => aInvert ? -2 : 2,
                _            => 0
            };

            object arg;
            var argTypes = _instructionCache[aOp].ArgumentTypes;
            if (argTypes[0] == typeof(Registers))
            {
                // Set the register to the required value.
                program.Add
                (
                    new CompilerIns(OpCode.MOV_LIT_REG,
                                        new object[] { value, r1 })
                );

                // This will become the jump instruction
                // condition argument below.
                arg = r1;
            }
            else
            {
                // This will become the jump instruction
                // condition argument below.
                arg = value;
            }

            var labels = new AsmLabel[2];
            if (aUseLabel)
            {
                labels[1] = new AsmLabel("GOOD", 1);
            }

            // If we are not using labels then we will go back and
            // update the jump position below.
            // Insert a placeholder value here for the time being.
            program.Add
            (
                new CompilerIns(aOp,
                                     new [] { arg, 0 },
                                     labels)
            );

            // In the jump not taken (inverted) case
            // this should execute.
            program.Add
            (
                new CompilerIns(OpCode.MOV_LIT_REG,
                                    new object[] { 0x321, r2 })
            );

            // Add a halt instruction to block execution
            // of the success instruction.
            program.Add(new CompilerIns(OpCode.HLT));

            if (aUseLabel)
            {
                program.Add
                (
                    new CompilerIns(OpCode.LABEL,
                                        new object[] { "GOOD" })
                );
            }
            else
            {
                // Calculate the address of the current instruction.
                // This effectively acts as a label.
                var jumpAddress =
                    CalculateDestinationAddress(program);

                // Go back and update the jump address in the instruction
                // from which the jump should or shouldn't occur.
                foreach (var entry in program)
                {
                    if (entry.Op != aOp)
                    {
                        continue;
                    }

                    entry.Args[1] = jumpAddress;
                    break;
                }
            }

            // In the jump is taken (normal) case this should execute.
            program.Add
            (
                new CompilerIns(OpCode.MOV_LIT_REG,
                                     new object[] { 0x123, r2 })
            );

            // Run the test program.
            Vm.Run(QuickCompile.RawCompile(program.ToArray()));

            // Check the result.
            Assert.IsTrue
            (
                Vm.Cpu.Registers[r2] == expected,
                $"Test OpCode '{aOp}'. " +
                $"Inverted = {aInvert}, Use Label = {aUseLabel} failed " +
                "to yield the correct result."
            );
        }

        private int CalculateDestinationAddress(IEnumerable<CompilerIns> aProgram)
        {
            var destAddress = Compiler.InitialAddress;

            foreach (var entry in aProgram)
            {
                destAddress += sizeof(OpCode);

                foreach (var insArg in entry.Args)
                {
                    if (insArg.GetType() == typeof(Registers))
                    {
                        destAddress += sizeof(Registers);
                    }
                    else
                    {
                        destAddress += Marshal.SizeOf(insArg);
                    }
                }
            }

            return destAddress;
        }
    }
}
