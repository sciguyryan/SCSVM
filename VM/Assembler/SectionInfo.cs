namespace VMCore.Assembler
{
    public class SectionInfo
    {
        public BinSections SectionId { get; }

        public int StartPosition { get; }

        public int Length { get; }

        public SectionInfo(BinSections aSecId, int aStartPos, int aLength)
        {
            SectionId = aSecId;
            StartPosition = aStartPos;
            Length = aLength;
        }
    }
}
