using Editor.Format;
using System.ComponentModel;
using System.Windows.Forms;

namespace Editor.Nodes
{
    public class FolderNode : TreeNode
    {
        public DirectoryEntry Entry { get; set; }

        public FolderNode(DirectoryEntry Entry)
        {
            this.Entry = Entry;

            Text = Entry.Name;
            ImageIndex = 1;
            SelectedImageIndex = 1;
        }

        [Category("Folder")]
        public string FolderName
        {
            get => Entry.Name;
        }
    }
}
