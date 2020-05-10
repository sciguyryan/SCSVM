using System;
using VMCore.VM;

namespace VMCore.Expressions
{
    public class NodeRegister : Node
    {
        private string _registerName;

        public NodeRegister(string registerName)
        {
            _registerName = registerName;
        }

        public override int Evaluate(CPU cpu)
        {
            // This should not usually happen, but
            // when we are compiling a binary
            // no CPU will be passed.
            // This is done in order to allow
            // for optimization analysis.
            if (cpu == null)
            {
                return -1;
            }

            var reg = 
                (Registers)Enum.Parse(typeof(Registers),
                                      _registerName);

            return cpu.Registers[reg];
        }
    }
}
