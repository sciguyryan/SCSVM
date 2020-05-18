using VMCore.VM.Core;

namespace VMCore.Expressions
{
    public abstract class Node
    {
        public abstract int Evaluate(CPU aCpu);
    }
}
