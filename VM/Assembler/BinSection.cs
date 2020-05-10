namespace VMCore.Assembler
{
    public class BinSection
    {
        public string Name { get; set; }

        public byte[] Raw { get; set; } = new byte[0];

        public override string ToString()
        {
            return Name;
        }
    }
}
