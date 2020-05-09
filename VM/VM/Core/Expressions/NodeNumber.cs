using System;

namespace VMCore.VM.Core.Expressions
{
    class NodeNumber : Node
    {
        private float _value;

        public NodeNumber(float value)
        {
            _value = value;
        }

        public override float Evaluate(CPU cpu)
        {
            return _value;
        }
    }
}
