using Editor.Format;
using System;
using System.ComponentModel;
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
    }
}
