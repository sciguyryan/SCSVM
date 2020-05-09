using System;

namespace VMCore.VM.Core.Expressions
{
    [Serializable]
    public abstract class Node
    {
        public abstract float Evaluate(CPU cpu);
    }
}
