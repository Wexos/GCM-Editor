using Editor.Format;
using Editor.Nodes;
using Editor.Properties;
using System;
using System.IO;
using System.Windows.Forms;

namespace Editor
{
    public partial class Form1 : Form
    {
        private GCM GCM;
        private FileStream GCMStream;

        public Form1()
        {
            InitializeComponent();

            IntPtr ImagePtr = Resources.Archive.GetHicon();
            Icon = System.Drawing.Icon.FromHandle(ImagePtr);

            ImageList ImageList = new ImageList();

            ImageList.Images.Add(Resources.Archive);
            ImageList.Images.Add(Resources.Folder);
            ImageList.Images.Add(Resources.CommonFile);

            treeView1.ImageList = ImageList;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog o = new OpenFileDialog()
            {
                Filter = "GCM Files (*.gcm; *.iso)|*.gcm; *.iso"
            };

            if (o.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            GCMStream = File.Open(o.FileName, FileMode.Open, FileAccess.ReadWrite);
            GCM = new GCM(GCMStream);

            treeView1.Nodes.Clear();
            treeView1.Nodes.Add(GCM.CreateTreeNode(o.FileName));
        }
        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GCM = null;

            GCMStream.Close();
            GCMStream = null;

            treeView1.Nodes.Clear();
        }
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            GCMStream?.Dispose();
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node is FolderNode Folder)
            {
                propertyGrid1.SelectedObject = new FolderProperty(Folder.Entry);
            }
            else if (e.Node is FileNode File)
            {
                propertyGrid1.SelectedObject = new FileProperty(File.Entry);
            }
            else
            {
                propertyGrid1.SelectedObject = null;
            }
        }
        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                treeView1.SelectedNode = e.Node;
            }
        }
    }
}
