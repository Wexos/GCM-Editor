using Editor.IO;

namespace Editor.Format
{
    public class DirectoryEntry
    {
        public DirectoryEntry(EndianBinaryReader Reader)
        {
            // Read
            _IsDirectory = Reader.ReadByte();
            StringOffset = Reader.ReadUInt24();
            Setting0 = Reader.ReadUInt32();
            Setting1 = Reader.ReadUInt32();
        }

        private byte _IsDirectory;
        public uint StringOffset { get; set; }
        public uint Setting0 { get; set; }
        public uint Setting1 { get; set; }

        public bool IsDirectory { get => _IsDirectory != 0; set => _IsDirectory = (byte)(value ? 1 : 0); }

        // For Folder
        public uint ParentOffset { get => Setting0; set => Setting0 = value; }
        public uint NextOffset { get => Setting1; set => Setting1 = value; }

        // For file
        public uint FileOffset { get => Setting0; set => Setting0 = value; }
        public uint FileSize { get => Setting1; set => Setting1 = value; }
    }
}
