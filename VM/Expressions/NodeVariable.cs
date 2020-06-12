namespace VMCore.Expressions
{
    internal class NodeVariable
        : Node
    {
        public Parser Parent;

        private readonly string _variableName;

        public NodeVariable(string aVariableName)
        {
            _variableName = aVariableName;
        }

        public override int Evaluate()
        {
            if (!Parent.Variables.TryGetValue(_variableName,
                                              out var value))
            {
                throw new ExprParserException
                (
                    $"Evaluate: variable '{_variableName}' " +
                    "could not be resolved."
                );
            }

            return value;
        }
    }
}
