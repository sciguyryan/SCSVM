using VMCore.VM;

namespace VMCore.Expressions
{
    public abstract class Node
    {
        public abstract int Evaluate(Cpu aCpu);
    }
}
