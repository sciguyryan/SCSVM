using System.Diagnostics;
using VMCore.VM.Core;

namespace VMCore.AsmParser
{
    internal class ParInstructionData
    {
        public object[] Arguments;
        public InsArgTypes[] ArgRefTypes;
        public string[] BoundLabels;

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
