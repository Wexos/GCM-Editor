using Editor.IO;
using System.IO;

namespace Editor.Format
{
    public class GCM
    {
        public Header Header { get; set; }


        public GCM(Stream Stream)
        {
            EndianBinaryReader Reader = new EndianBinaryReader(Stream, Endianness.BigEndian);

            // Read header
            Header = new Header(Reader);
        }
    }
}
