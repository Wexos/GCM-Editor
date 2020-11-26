namespace Editor.Format
{
    public struct OffsetSizePair
    {
        public long Offset { get; set; }
        public long Size { get; set; }

        public OffsetSizePair(long Offset, long Size)
        {
            this.Offset = Offset;
            this.Size = Size;
        }
    }
}
