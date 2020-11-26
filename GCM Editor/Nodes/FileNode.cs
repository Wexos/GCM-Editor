using Editor.Format;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Editor.Nodes
{
    public class FileNode : TreeNode
    {
        public DirectoryEntry Entry { get; set; }

        public FileNode(DirectoryEntry Entry)
        {
            this.Entry = Entry;

            Text = Entry.Name;
            ImageIndex = 2;
            SelectedImageIndex = 2;
        }

        public string GetFileFilter()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("All Files (*.*)|*.*");

            string Extension = Path.GetExtension(Entry.Name);

            if (!string.IsNullOrEmpty(Extension))
            {
                Extension = Extension.TrimStart('.');

                string Lower = Extension.ToLower();
                string Upper = Extension.ToUpper();

                sb.Insert(0, $"{Upper} Files (*.{Lower})|*.{Lower}|");
            }

            return sb.ToString();
        }
    }
}
