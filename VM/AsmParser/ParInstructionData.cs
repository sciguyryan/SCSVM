using VMCore.VM.Core;

namespace VMCore.AsmParser
{
    internal class ParInstructionData
    {
        public object[] Arguments { get; }

        public InsArgTypes[] ArgRefTypes { get; }

        public string[] BoundLabels { get; }

        public ParInstructionData(object[] aArgs,
                                  InsArgTypes[] aRefTypes,
                                  string[] aLabels)
        {
            Arguments = aArgs;
            ArgRefTypes = aRefTypes;
            BoundLabels = aLabels;
        }
    }
}
