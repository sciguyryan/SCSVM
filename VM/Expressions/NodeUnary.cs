using System;
using VMCore.VM.Core;

namespace VMCore.Expressions
{
    class NodeUnary
        : Node
    {
        private Node _rhs;
        private OpTypes _op;

        public NodeUnary(Node aRhs, OpTypes aOp)
        {
            _rhs = aRhs;
            _op = aOp;
        }

        public override int Evaluate(CPU aCpu)
        {
            var rhsVal = _rhs.Evaluate(aCpu);

            // A unary + as that is just the same
            // as the original value.
            return _op switch
            {
                OpTypes.Negate => -rhsVal,
                _              => throw new NotImplementedException(),
            };
        }
    }
}
