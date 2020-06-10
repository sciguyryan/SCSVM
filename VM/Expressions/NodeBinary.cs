using System;

namespace VMCore.Expressions
{
    internal class NodeBinary
        : Node
    {
        private readonly Node _lhs;
        private readonly Node _rhs;
        private readonly OpTypes _op;

        public NodeBinary(Node aLhs, Node aRhs, OpTypes aOp)
        {
            _lhs = aLhs;
            _rhs = aRhs;
            _op = aOp;
        }

        public override int Evaluate()
        {
            var lhsVal = _lhs.Evaluate();
            var rhsVal = _rhs.Evaluate();

            return _op switch
            {
                OpTypes.Add      => lhsVal + rhsVal,
                OpTypes.Subtract => lhsVal - rhsVal,
                OpTypes.Multiply => lhsVal * rhsVal,
                OpTypes.Divide   => lhsVal / rhsVal,
                _                => throw new NotImplementedException(),
            };
        }
    }
}
