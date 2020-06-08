namespace VMCore.Assembler
{
    public class BinSection
    {
        public BinSections SectionId { get; set; }

        public string Name => SectionId.ToString();

        public byte[] Raw { get; set; } = new byte[0];

        public int EntryPoint { get; set; } = 0;

        public override string ToString()
        {
            return Name;
        }
    }
}
