using System;
using System.Collections.Generic;
using System.Linq;
using VMCore;
using VMCore.Assembler;
using VMCore.VM;

namespace UnitTests.Instructions
{
    public static class TestUtilties
    {
        /// <summary>
        /// A method to generate the required instructions to test a given opcode.
        /// </summary>
        /// <typeparam name="T">The type of input expected for non-Register parameters.</typeparam>
        /// <param name="op">The opcode to be tested.</param>
        /// <param name="values">An array of values to be passed to as arguments to the instruction.</param>
        /// <returns>A list of QuickInstruction objects to be executed for the test.</returns>
        public static List<QuickIns> Generate<T>(OpCode aOp, T[] aVals)
        {
            var instructionList
                = new List<QuickIns>();

            // The types of argument that we expect for this
            // instruction.
            var types =
                ReflectionUtils.InstructionCache[aOp].ArgumentTypes;

            // The number of arguments that
            // take the Register type.
            var registerArgs =
                types.Count(x => x == typeof(Registers));

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
            for (int i = 0; i < registerArgs; i++)
            {
                instructionList.Add(new QuickIns(OpCode.MOV_LIT_REG, new object[] { argQueue.Dequeue(), (byte)i }));
            }

            // Build the instruction argument
            // list that we are actually testing.
            var args = new List<object>();
            var registerID = 0;
            for (var i = 0; i < types.Length; i++)
            {
                switch (types[i])
                {
                    case Type _ when types[i] == typeof(Registers):
                        {
                            // Keep track of which register
                            // IDs have already been used.
                            // Argument 1 will point to register 0
                            // (R1), argument 2 will point to
                            // register 1 (R2), etc.
                            args.Add((Registers)registerID);
                            ++registerID;
                        }
                        break;

                    case Type _ when types[i] == typeof(int):
                        {
                            // Ensure that we are not reusing
                            // the same values.
                            args.Add(Convert.ChangeType(argQueue.Dequeue(),
                                                        typeof(int)));
                        }
                        break;

                    default:
                        throw new NotSupportedException($"GenerateProgram: the type {types[i]} was passed as an argument type, but no support has been provided for that type.");
                        break;
                }
            }

            instructionList.Add(new QuickIns(aOp, args.ToArray()));

            return instructionList;
        }
    }
}
