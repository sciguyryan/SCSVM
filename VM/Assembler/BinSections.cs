namespace VMCore.Assembler
{
    public enum BinSections
    {
        /// <summary>
        /// Meta data for the binary file.
        /// </summary>
        Meta,
        /// <summary>
        /// Code data for the binary file.
        /// </summary>
        Text,
        /// <summary>
        /// Hard coded data for the binary file.
        /// </summary>
        Data,
        /// <summary>
        /// Read-only data for the binary file.
        /// </summary>
        RData,
        /// <summary>
        /// Variable definition data for the binary file.
        /// </summary>
        BSS,
    }
}
