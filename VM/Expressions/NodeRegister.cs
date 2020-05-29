#nullable enable

using System;
using VMCore.VM;
using VMCore.VM.Core;

namespace VMCore.Expressions
{
    public class NodeRegister
        : Node
    {
        private readonly string _registerName;

        public NodeRegister(string aRegisterName)
        {
            _registerName = aRegisterName;
        }

        public override int Evaluate(Cpu aCpu)
        {
            // This should not usually happen, but
            // when we are compiling a binary
            // no CPU will be passed.
            // This is done in order to allow
            // for optimization analysis.
            if (aCpu is null)
            {
                return -1;
            }

            var reg = 
                (Registers)Enum.Parse(typeof(Registers),
                                      _registerName);

            return aCpu.Registers[reg];
        }
    }
}
