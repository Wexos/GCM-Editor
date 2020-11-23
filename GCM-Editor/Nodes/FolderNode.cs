using Editor.Format;
using System.ComponentModel;
using System.Windows.Forms;

namespace Editor.Nodes
{
    public class FolderNode : TreeNode
    {
        private DirectoryEntry Entry;

        public FolderNode(DirectoryEntry Entry)
        {
            this.Entry = Entry;
            Text = Entry.Name;
        }

        [Category("Folder")]
        public string FolderName
        {
            get => Entry.Name;
        }
    }
}
