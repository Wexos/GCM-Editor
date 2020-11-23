using Editor.IO;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Editor.Format
{
    public class GCM
    {
        public Header Header { get; set; }
        public List<DirectoryEntry> Entries { get; set; }

        public GCM(Stream Stream)
        {
            EndianBinaryReader Reader = new EndianBinaryReader(Stream, Endianness.BigEndian);

            // Read header
            Header = new Header(Reader);

            // Read file system
            Reader.Position = Header.FileSystemOffset;

            DirectoryEntry Root = new DirectoryEntry(Reader);
            Entries = new List<DirectoryEntry>();

            for (int i = 1; i < Root.Setting1; i++)
            {
                Entries.Add(new DirectoryEntry(Reader));
            }

            // Read names
            long NameStart = Reader.Position;

            for (int i = 0; i < Entries.Count; i++)
            {
                Reader.Position = NameStart + Entries[i].NameOffset;
                Entries[i].Name = Reader.ReadStringNT(Encoding.ASCII);
            }
        }
    }
}
