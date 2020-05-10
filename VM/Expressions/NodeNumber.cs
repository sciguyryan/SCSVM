using VMCore.VM;

namespace VMCore.Expressions
{
    class NodeNumber : Node
    {
        private int _value;

        public NodeNumber(int value)
        {
            _value = value;
        }

        public override int Evaluate(CPU cpu)
        {
            return _value;
        }
    }
}
