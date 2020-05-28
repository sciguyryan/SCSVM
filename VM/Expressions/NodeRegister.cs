using System;
using VMCore.VM;

namespace VMCore.Expressions
{
    public class NodeRegister
        : Node
    {
        private string _registerName;

        public NodeRegister(string aRegisterName)
        {
            _registerName = aRegisterName;
        }

        public override int Evaluate(Cpu aCpu)
        {
            // This should not usually happen, but
            // when we are compiling a binary
            // no Cpu will be passed.
            // This is done in order to allow
            // for optimization analysis.
            if (aCpu == null)
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
