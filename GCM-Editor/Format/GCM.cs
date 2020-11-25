using Editor.IO;
using Editor.Nodes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Editor.Format
{
    public class GCM
    {
        public Header Header { get; set; }
        public List<DirectoryEntry> Entries { get; set; }

        public const uint FileSize = 0x57058000;
        public const uint FileAlignment = 0x80;

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

        public TreeNode CreateTreeNode(string FileName)
        {
            GCMNode Root = new GCMNode(this, FileName);

            int i = 1;
            ParseFileSystem(Root, ref i, Entries.Count);

            return Root;
        }
        private void ParseFileSystem(TreeNode Parent, ref int i, int End)
        {
            while (i < End)
            {
                DirectoryEntry Entry = Entries[i - 1];
                i++;

                if (Entry.IsDirectory)
                {
                    FolderNode Node = new FolderNode(Entry);

                    Parent.Nodes.Add(Node);

                    ParseFileSystem(Node, ref i, (int)Entry.NextID);
                }
                else
                {
                    Parent.Nodes.Add(new FileNode(Entry));
                }
            }
        }

        public long AvailableSizeOfFile(DirectoryEntry File, Stream GCMStream)
        {
            long Size = long.MaxValue;
            bool IsLastFile = true;

            for (int i = 0; i < Entries.Count; i++)
            {
                if (!Entries[i].IsDirectory && Entries[i] != File && Entries[i].FileOffset >= File.FileOffset)
                {
                    if (File.FileOffset == Entries[i].FileOffset)
                    {
                        // Special case which is currently not handled
                        throw new NotImplementedException();
                    }

                    IsLastFile = false;
                }
            }

            if (IsLastFile)
            {
                return GCMStream.Length - File.FileOffset;
            }

            return Size;
        }
    }
}
