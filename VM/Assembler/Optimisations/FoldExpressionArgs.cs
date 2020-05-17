using System;
using VMCore.VM;
using VMCore.Expressions;
using VMCore.VM.Instructions;

namespace VMCore.Assembler.Optimisations
{
    class FoldExpressionArg
    {
        /// <summary>
        /// Optimize an expression by attempting to fold
        /// (simplify) an expression into a single literal
        /// value.
        /// </summary>
        /// <param name="aOp">The opcode of the instruction to be folded.</param>
        /// <param name="aArgIndex">The index of the argument to be folded.</param>
        /// <param name="ins">The instruction instance for the opcode.</param>
        /// <param name="aArg">The data for the opcode argument.</param>
        /// <returns>A tuple of the output opcode, argument type and argument data.</returns>
        public static (OpCode, Type, object) FoldExpression(OpCode aOp,
                                                            int aArgIndex,
                                                            Instruction aIns,
                                                            object aArg)
        {
            if (aIns.ExpressionArgType(aArgIndex) == null)
            {
                throw new Exception($"FoldExpression: argument {aArgIndex} for opcode {aOp} is not an expression and so should not have been passed here.");
            }

            OpCode opCode = aOp;

            // Expression arguments are always of type string
            // unless they can be flattened.
            Type outArgType = typeof(string);

            // Get rid of any white-spaces that
            // we do not need here.
            // This will make parsing these strings
            // faster.
            var s = Utils.StripWhiteSpaces((string)aArg);
            object output;

            // Next we will see if we can simplify
            // this expression before encoding it.
            var p = new Parser(s);
            var n = p.ParseExpression();

            // Now that we have parsed the expression
            // we can determine if this expression
            // can be reduced to a pure literal.
            // If it can then this will be less
            // work later.
            if (p.IsSimple)
            {
                // We can simplify this expression
                // to a pure value.
                // We can pass a null CPU
                // here as it will not be used
                // at all for a simple expression.
                var val = n.Evaluate(null);

                // The case of a valid simplification
                // the new type of the argument
                // becomes the expression output type.
                outArgType = aIns.ExpressionArgType(aArgIndex);

                // In the case of a valid simplification
                // we can subtract one from the opcode and
                // fold the value.
                // This would change a MOV_EXPR_OFF_REG
                // into a MOV_LIT_OFF_REG.
                if (outArgType == typeof(int))
                {
                    opCode = aOp - 1;
                    output = (int)val;
                }
                else if (outArgType == typeof(float))
                {
                    opCode = aOp - 1;
                    output = (float)val;
                }
                else
                {
                    throw new NotSupportedException($"FoldExpression: the type {outArgType} was passed as the expression argument type, but no support has been provided for that type.");
                }
            }
            else
            {
                // We cannot simplify this expression
                // so it will have to be evaluated
                // at runtime.
                output = s;
            }

            return (opCode, outArgType, output);
        }
    }
}
