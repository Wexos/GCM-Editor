using Editor.Format;
using System.IO;
using System.Windows.Forms;

namespace Editor.Nodes
{
    public class GCMNode : TreeNode
    {
        private GCM GCM;

        public GCMNode(GCM GCM, string FileName)
        {
            this.GCM = GCM;

            Text = Path.GetFileName(FileName);

            ImageIndex = 0;
            SelectedImageIndex = 0;
        }
    }
}
