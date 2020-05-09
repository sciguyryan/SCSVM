using System;

namespace VMCore.VM.Core.Expressions
{
    class NodeBinary : Node
    {
        private Node _lhs;
        private Node _rhs;
        private OpTypes _op;

        public NodeBinary(Node lhs, Node rhs, OpTypes op)
        {
            _lhs = lhs;
            _rhs = rhs;
            _op = op;
        }

        public override float Evaluate(CPU cpu)
        {
            var lhsVal = _lhs.Evaluate(cpu);
            var rhsVal = _rhs.Evaluate(cpu);

            return _op switch
            {
                OpTypes.Add         => lhsVal + rhsVal,
                OpTypes.Subtract    => lhsVal - rhsVal,
                OpTypes.Multiply    => lhsVal * rhsVal,
                OpTypes.Divide      => lhsVal / rhsVal,
                _                   => throw new NotImplementedException(),
            };
        }
    }
}
