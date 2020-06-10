namespace VMCore.Expressions
{
    internal class NodeNumber
        : Node
    {
        private readonly int _value;

        public NodeNumber(int aValue)
        {
            _value = aValue;
        }

        public override int Evaluate()
        {
            return _value;
        }
    }
}
