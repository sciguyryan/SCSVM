using VMCore.VM;

namespace VMCore.Expressions
{
    class NodeNumber
        : Node
    {
        private int _value;

        public NodeNumber(int aValue)
        {
            _value = aValue;
        }

        public override int Evaluate(CPU aCpu)
        {
            return _value;
        }
    }
}
