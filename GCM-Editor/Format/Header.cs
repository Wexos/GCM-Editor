using Editor.IO;
using System.Text;

namespace Editor.Format
{
    public class Header
    {
        public Header(EndianBinaryReader Reader)
        {
            // Read
            GameID = Reader.ReadString(4, Encoding.ASCII);
            CompanyID = Reader.ReadUInt16();
            DiskID = Reader.ReadByte();
            Version = Reader.ReadByte();
            AudioStreaming = Reader.ReadByte();
            StreamBufferSize = Reader.ReadByte();
            Reader.ReadBytes(0x0E);
            WiiMagic = Reader.ReadUInt32();
            GCMagic = Reader.ReadUInt32();
            Name = Reader.ReadFixedString(0x3E0, Encoding.ASCII);
            DebugOffset = Reader.ReadUInt32();
            DebugAddress = Reader.ReadUInt32();
            Reader.ReadBytes(0x18);
            DOLOffset = Reader.ReadUInt32();
            FileSystemOffset = Reader.ReadUInt32();
            FileSystemSize = Reader.ReadUInt32();
            FileSystemMaxSize = Reader.ReadUInt32();
            UnknownAddress = Reader.ReadUInt32();
            FileDataStartOffset = Reader.ReadUInt32();
            UnknownOffset2 = Reader.ReadUInt32();
        }

        public string GameID { get; set; }
        public ushort CompanyID { get; set; }
        public byte DiskID { get; set; }
        public byte Version { get; set; }
        public byte AudioStreaming { get; set; }
        public byte StreamBufferSize { get; set; }
        // byte[0x0E]
        public uint WiiMagic { get; set; }
        public uint GCMagic { get; set; }
        public string Name { get; set; }
        public uint DebugOffset { get; set; }
        public uint DebugAddress { get; set; }
        // byte[0x18]
        public uint DOLOffset { get; set; }
        public uint FileSystemOffset { get; set; }
        public uint FileSystemSize { get; set; }
        public uint FileSystemMaxSize { get; set; }
        public uint UnknownAddress { get; set; }
        public uint FileDataStartOffset { get; set; }
        public uint UnknownOffset2 { get; set; }
    }
}
