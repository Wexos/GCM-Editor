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
        public const uint FileAlignment = 4;

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

        public GCMNode CreateTreeNode(string FileName)
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

        public byte[] ReadFile(DirectoryEntry File, Stream GCMStream)
        {
            byte[] Data = new byte[File.FileSize];

            GCMStream.Position = File.FileOffset;
            GCMStream.Read(Data, 0, Data.Length);

            return Data;
        }

        public void ExportFile(DirectoryEntry File, Stream GCMStream, Stream Output)
        {
            byte[] Data = ReadFile(File, GCMStream);
            Output.Write(Data, 0, Data.Length);
        }
        public bool ReplaceFile(DirectoryEntry File, Stream GCMStream, byte[] FileData)
        {
            long Offset = GetNewOffsetOfFileData(File, GCMStream, FileData.Length);

            if (Offset < 0)
            {
                return false;
            }

            // Write file data
            GCMStream.Position = Offset;
            GCMStream.Write(FileData, 0, FileData.Length);

            // Change size and offset
            File.FileOffset = (uint)Offset;
            File.FileSize = (uint)FileData.Length;

            EndianBinaryWriter Writer = new EndianBinaryWriter(GCMStream, Endianness.BigEndian);

            Writer.Position = File.FileAddress;
            File.Write(Writer);

            return true;
        }

        public void ExportDirectory(FolderNode Folder, Stream GCMStream, string OutputFolder)
        {
            if (!Directory.Exists(OutputFolder))
            {
                Directory.CreateDirectory(OutputFolder);
            }

            for (int i = 0; i < Folder.Nodes.Count; i++)
            {
                if (Folder.Nodes[i] is FolderNode FolderNode)
                {
                    ExportDirectory(FolderNode, GCMStream, Path.Combine(OutputFolder, FolderNode.Entry.Name));
                }
                else if (Folder.Nodes[i] is FileNode FileNode)
                {
                    using (FileStream Output = File.Open(Path.Combine(OutputFolder, FileNode.Entry.Name), FileMode.Create, FileAccess.Write))
                    {
                        ExportFile(FileNode.Entry, GCMStream, Output);
                    }
                }
            }
        }

        public long GetNewOffsetOfFileData(DirectoryEntry File, Stream GCMStream, long NewFileSize)
        {
            //if (NewFileSize <= File.FileSize)
            //{
            //    return File.FileOffset;
            //}

            // First check if there is space between the file and the next one
            long NextOffset = long.MaxValue;

            for (int i = 0; i < Entries.Count; i++)
            {
                if (Entries[i].IsDirectory || File == Entries[i] || Entries[i].FileSize == 0)
                {
                    continue;
                }

                if (Entries[i].FileOffset >= File.FileOffset && Entries[i].FileOffset < NextOffset)
                {
                    NextOffset = Entries[i].FileOffset;
                }
            }

            if (NextOffset < GCMStream.Length)
            {
                long AvailableSize = NextOffset - File.FileOffset;

                if (NewFileSize <= AvailableSize)
                {
                    return File.FileOffset;
                }
            }

            // Check all data which is available
            long AlignedFileSize = NewFileSize;

            while (AlignedFileSize % FileAlignment != 0)
            {
                AlignedFileSize++;
            }

            // <Offset, size>
            List<OffsetSizePair> AvailableRegions = new List<OffsetSizePair>();
            long FileDataStart = Header.FileDataStartOffset; // Is this how it works?

            AvailableRegions.Add(new OffsetSizePair(FileDataStart, GCMStream.Length - FileDataStart));

            // We assume no file data is overlapping (which it shouldn't)
            for (int i = 0; i < Entries.Count; i++)
            {
                if (Entries[i].IsDirectory || File == Entries[i] || Entries[i].FileSize == 0)
                {
                    continue;
                }

                uint AlignedSize = Entries[i].FileSize;

                // Find and split region
                int Index = AvailableRegions.FindIndex((x) => Entries[i].FileOffset >= x.Offset && Entries[i].FileOffset < x.Offset + x.Size);

                // Index should always be valid
                OffsetSizePair Region = AvailableRegions[Index];
                AvailableRegions.RemoveAt(Index);

                long RegionEnd = Region.Offset + Region.Size;
                long FileEnd = Entries[i].FileOffset + Entries[i].FileSize;

                OffsetSizePair[] SubRegions = new OffsetSizePair[]
                {
                    new OffsetSizePair(Region.Offset, Entries[i].FileOffset - Region.Offset),
                    new OffsetSizePair(Entries[i].FileOffset, RegionEnd - Entries[i].FileOffset)
                };

                for (int j = 0; j < SubRegions.Length; j++)
                {
                    // Align
                    while (SubRegions[j].Offset % 4 != 0)
                    {
                        // Increase offset => decrease size
                        SubRegions[j].Offset++;
                        SubRegions[j].Size--;
                    }

                    if (SubRegions[j].Size >= FileAlignment)
                    {
                        AvailableRegions.Add(SubRegions[j]);
                    }
                }
            }

            // Find available
            int AvailableIndex = AvailableRegions.FindIndex((x) => x.Size >= AlignedFileSize);

            if (AvailableIndex == -1)
            {
                return -1;
            }
            else
            {
                return AvailableRegions[AvailableIndex].Offset;
            }
        }

        public long GetFirstFileOffset()
        {
            long Offset = long.MaxValue;

            for (int i = 0; i < Entries.Count; i++)
            {
                if (!Entries[i].IsDirectory)
                {
                    Offset = Math.Min(Offset, Entries[i].FileOffset);
                }
            }

            return Offset;
        }
        public long GetLastFileOffset()
        {
            long Offset = long.MinValue;

            for (int i = 0; i < Entries.Count; i++)
            {
                if (!Entries[i].IsDirectory)
                {
                    Offset = Math.Max(Offset, Entries[i].FileOffset);
                }
            }

            return Offset;
        }
    }
}
