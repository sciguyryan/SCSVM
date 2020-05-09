using VMCore.VM;

namespace VMCore.Expressions
{
    public abstract class Node
    {
        public abstract float Evaluate(CPU cpu);
    }
}
