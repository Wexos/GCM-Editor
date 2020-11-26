using Editor.IO;
using System;

namespace Editor.Format
{
    public class DirectoryEntry
    {
        public DirectoryEntry(EndianBinaryReader Reader)
        {
            FileAddress = Reader.Position;

            // Read
            _IsDirectory = Reader.ReadByte();
            NameOffset = Reader.ReadUInt24();
            Setting0 = Reader.ReadUInt32();
            Setting1 = Reader.ReadUInt32();
        }
        public void Write(EndianBinaryWriter Writer)
        {
            // Write
            Writer.WriteByte(_IsDirectory);
            Writer.WriteUInt24(NameOffset);
            Writer.WriteUInt32(Setting0);
            Writer.WriteUInt32(Setting1);
        }

        private byte _IsDirectory;
        public uint NameOffset { get; set; }
        public uint Setting0 { get; set; }
        public uint Setting1 { get; set; }

        public string Name { get; set; }

        public bool IsDirectory { get => _IsDirectory != 0; set => _IsDirectory = (byte)(value ? 1 : 0); }

        // For Folder
        public uint ParentID { get => Setting0; set => Setting0 = value; }
        public uint NextID { get => Setting1; set => Setting1 = value; }

        // For file
        public uint FileOffset { get => Setting0; set => Setting0 = value; }
        public uint FileSize { get => Setting1; set => Setting1 = value; }

        public long FileAddress { get; private set; }
    }
}
