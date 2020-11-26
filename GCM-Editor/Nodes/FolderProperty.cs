using Editor.Format;
using System.ComponentModel;

namespace Editor.Nodes
{
    public class FolderProperty
    {
        private DirectoryEntry Entry;

        public FolderProperty(DirectoryEntry Entry)
        {
            this.Entry = Entry;
        }

        [Category("Folder")]
        public string Name
        {
            get => Entry.Name;
        }
    }
}
