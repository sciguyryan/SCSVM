using System;
using VMCore.VM.Core;

namespace VMCore.Expressions
{
    class NodeBinary
        : Node
    {
        private Node _lhs;
        private Node _rhs;
        private OpTypes _op;

        public NodeBinary(Node aLhs, Node aRhs, OpTypes aOp)
        {
            _lhs = aLhs;
            _rhs = aRhs;
            _op = aOp;
        }

        public override int Evaluate(CPU aCpu)
        {
            var lhsVal = _lhs.Evaluate(aCpu);
            var rhsVal = _rhs.Evaluate(aCpu);

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
