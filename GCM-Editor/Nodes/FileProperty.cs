using Editor.Format;
using System;
using System.ComponentModel;

namespace Editor.Nodes
{
    public class FileProperty
    {
        private DirectoryEntry Entry;

        public FileProperty(DirectoryEntry Entry)
        {
            this.Entry = Entry;
        }

        [Category("File")]
        public string FileName
        {
            get => Entry.Name;
        }

        [Category("File")]
        public string FileOffset
        {
            get => $"0x{Entry.FileOffset:X8}";
        }

        [Category("File")]
        public string FileSize
        {
            get
            {
                const double Kilo = 1024d;
                const double Mega = 1024d * 1024d;
                const double Giga = 1024d * 1024d * 1024d;
                const double Tera = 1024d * 1024d * 1024d * 1024d;

                long FileSize = Entry.FileSize;

                if (FileSize > Tera)
                {
                    double Size = FileSize / Tera;
                    return $"{Math.Round(Size, 1)} TB";
                }
                else if (FileSize > Giga)
                {
                    double Size = FileSize / Giga;
                    return $"{Math.Round(Size, 1)} GB";
                }
                else if (FileSize > Mega)
                {
                    double Size = FileSize / Mega;
                    return $"{Math.Round(Size, 1)} MB";
                }
                else if (FileSize > Kilo)
                {
                    double Size = FileSize / Kilo;
                    return $"{Math.Round(Size, 1)} kB";
                }
                else
                {
                    return $"{FileSize} B";
                }
            }
        }
    }
}
