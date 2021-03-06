﻿using System;
using System.Collections.Generic;
using System.Linq;
using VMCore.Assembler;
using VMCore.VM;
using VMCore.VM.Core;
using VMCore.VM.Core.Register;
using VMCore.VM.Core.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Instructions.Helpers
{
    public static class TestUtilities
    {
        public static readonly VMCore.AsmParser.AsmParser AsmParser =
            new VMCore.AsmParser.AsmParser();

        /// <summary>
        /// A method to generate the required instructions
        /// to test a given opcode.
        /// </summary>
        /// <typeparam name="T">
        /// The type of input expected for non-Register parameters.
        /// </typeparam>
        /// <param name="aOp">The opcode to be tested.</param>
        /// <param name="aVals">
        /// An array of values to be passed to as arguments to
        /// the instruction.
        /// </param>
        /// <returns>A list of QuickInstruction objects to be
        /// executed for the test.</returns>
        public static CompilerIns[] Generate<T>(OpCode aOp, T[] aVals)
        {
            var instructionList
                = new List<CompilerIns>();

            // The types of argument that we expect for this
            // instruction.
            var types =
                ReflectionUtils.InstructionCache[aOp].ArgumentTypes;

            // The number of arguments that
            // take the Register type.
            var registerArgs =
                types.Count(aX => aX == typeof(Registers));

            // Load our arguments into a queue
            // to avoid things being accidentally
            // reused.
            var argQueue = new Queue<T>(aVals);
            foreach (var arg in aVals)
            {
                argQueue.Enqueue(arg);
            }

            // Load the registers with the test values.
            // We will create one load argument for each
            // register that we expect to use.
            // Registers are incrementally assigned
            // to avoid overlaps.
            for (var i = 0; i < registerArgs; i++)
            {
                instructionList.Add
                (
                    new CompilerIns
                    (
                        OpCode.MOV_LIT_REG, 
                        new object[] { argQueue.Dequeue(), (byte)i }
                    )
                );
            }

            // Build the instruction argument
            // list that we are actually testing.
            var args = new List<object>();
            var registerId = 0;
            foreach (var t in types)
            {
                switch (t)
                {
                    case { } _ when t == typeof(Registers):
                    {
                        // Keep track of which register
                        // IDs have already been used.
                        // Argument 1 will point to register 0
                        // (R1), argument 2 will point to
                        // register 1 (R2), etc.
                        args.Add((Registers)registerId);
                        ++registerId;
                    }
                    break;

                    case { } _ when t == typeof(int):
                    {
                        // Ensure that we are not reusing
                        // the same values.
                        var o =
                            Convert.ChangeType(argQueue.Dequeue(),
                                               typeof(int));
                        args.Add(o);
                    }
                    break;

                    default:
                        throw new NotSupportedException
                        (
                            $"GenerateProgram: the type {t} " +
                            "was passed as an argument type, but no " +
                            "support has been provided for that type."
                        );
                }
            }

            instructionList.Add(new CompilerIns(aOp, args.ToArray()));
            instructionList.Add(new CompilerIns(OpCode.HLT));

            return instructionList.ToArray();
        }


        public static void ExecutionOrderTest(VirtualMachine aVm,
                                              OpCode[] aOpCodes,
                                              string[] aProgram)
        {
            var pStr = string.Join(Environment.NewLine, aProgram);

            var program = 
                AsmParser.Parse(pStr).CodeSectionData.ToArray();

            aVm.LoadAndInitialize(QuickCompile.RawCompile(program));

            var i = 0;
            var ins = aVm.Cpu.Step();
            while (!(ins is null))
            {
                Assert.IsTrue
                (
                    ins.OpCode == aOpCodes[i],
                    $"OpCode at position {i} mismatched. " +
                    $"Expected {ins.OpCode}, got {aOpCodes[i]}."
                );

                ins = aVm.Cpu.Step();
                ++i;
            }
        }
    }
}
