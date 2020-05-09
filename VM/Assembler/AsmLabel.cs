namespace VMCore.Assembler
{
    public class AsmLabel
    {
        /// <summary>
        /// The name of this label.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The argument to which this label is bound.
        /// </summary>
        public int BoundArgumentIndex { get; private set; }

        public AsmLabel(string name, int boundArgumnetIndex)
        {
            Name = name;
            BoundArgumentIndex = boundArgumnetIndex;
        }
    }
}
