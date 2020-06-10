using System;

namespace VMCore.Expressions
{
    internal class NodeUnary
        : Node
    {
        private readonly Node _rhs;
        private readonly OpTypes _op;

        public NodeUnary(Node aRhs, OpTypes aOp)
        {
            _rhs = aRhs;
            _op = aOp;
        }

        public override int Evaluate()
        {
            var rhsVal = _rhs.Evaluate();

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
