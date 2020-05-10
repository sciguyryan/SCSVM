using System;
using VMCore.VM;

namespace VMCore.Expressions
{
    class NodeUnary : Node
    {
        private Node _rhs;
        private OpTypes _op;

        public NodeUnary(Node rhs, OpTypes op)
        {
            _rhs = rhs;
            _op = op;
        }

        public override int Evaluate(CPU cpu)
        {
            var rhsVal = _rhs.Evaluate(cpu);

            // A unary + as that is just the same
            // as the original value.
            return _op switch
            {
                OpTypes.Negate   => -rhsVal,
                _                => throw new NotImplementedException(),
            };
        }
    }
}
