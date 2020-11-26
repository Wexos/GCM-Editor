using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Editor
{
    public class RecentFileList
    {
        public List<string> Files { get; set; }

        private const string FileName = "RecentFiles.txt";

        public RecentFileList()
        {
            Files = new List<string>();
        }

        public void LoadFromDisk()
        {
            Files.Clear();

            if (File.Exists(FileName))
            {
                using (FileStream Input = File.Open(FileName, FileMode.Open, FileAccess.Read))
                {
                    StreamReader Reader = new StreamReader(Input);
                    string Line;

                    while ((Line = Reader.ReadLine()) != null)
                    {
                        if (File.Exists(Line))
                        {
                            Files.Add(Line);
                        }
                    }
                }
            }
        }
        public void SaveToDisk()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < Files.Count; i++)
            {
                sb.AppendLine(Files[i]);
            }

            File.WriteAllText(FileName, sb.ToString());
        }

        public void AddFile(string FileName)
        {
            if (Files.Contains(FileName))
            {
                Files.Remove(FileName);
            }

            Files.Insert(0, FileName);
        }
    }
}
