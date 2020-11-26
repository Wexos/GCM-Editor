using Editor.Format;
using Editor.Nodes;
using Editor.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Editor
{
    public partial class Form1 : Form
    {
        private GCM GCM;
        private string GCMFilePath;

        private RecentFileList RecentFileList;

        public Form1()
        {
            InitializeComponent();

            IntPtr ImagePtr = Resources.Archive.GetHicon();
            Icon = System.Drawing.Icon.FromHandle(ImagePtr);

            // Image list
            ImageList ImageList = new ImageList();

            ImageList.Images.Add(Resources.Archive);
            ImageList.Images.Add(Resources.Folder);
            ImageList.Images.Add(Resources.CommonFile);

            treeView1.ImageList = ImageList;

            // Recent files
            RecentFileList = new RecentFileList();
            RecentFileList.LoadFromDisk();

            for (int i = 0; i < RecentFileList.Files.Count; i++)
            {
                ToolStripMenuItem Item = new ToolStripMenuItem(RecentFileList.Files[i]);
                Item.Click += recentFile_Click;

                recentFilesToolStripMenuItem.DropDownItems.Add(Item);
            }
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

            OpenGCM(o.FileName);
        }
        private void recentFile_Click(object sender, EventArgs e)
        {
            string FileName = ((ToolStripItem)sender).Text;

            if (!File.Exists(FileName))
            {
                MessageBox.Show("File not found. It has either been moved or deleted.", "GCM", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            OpenGCM(FileName);
        }
        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GCM = null;
            GCMFilePath = null;

            treeView1.Nodes.Clear();
        }
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
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

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(DataFormats.FileDrop, false) is IList<string> FileDrops)
            {
                for (int i = 0; i < FileDrops.Count; i++)
                {
                    if (File.Exists(FileDrops[i]))
                    {
                        OpenGCM(FileDrops[i]);
                        break;
                    }
                }
            }
        }
        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void exportFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileNode Node = (FileNode)treeView1.SelectedNode;

            SaveFileDialog s = new SaveFileDialog()
            {
                Filter = Node.GetFileFilter(),
                FileName = Node.Entry.Name
            };

            if (s.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            using (FileStream Output = File.Open(s.FileName, FileMode.Create, FileAccess.Write),
                GCMStream = OpenGCMStream())
            {
                GCM.ExportFile(Node.Entry, GCMStream, Output);
            }
        }
        private void replaceFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileNode Node = (FileNode)treeView1.SelectedNode;

            OpenFileDialog o = new OpenFileDialog()
            {
                Filter = Node.GetFileFilter(),
                FileName = Node.Entry.Name
            };

            if (o.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            byte[] Data = File.ReadAllBytes(o.FileName);

            using (FileStream GCMStream = OpenGCMStream())
            {
                if (GCM.ReplaceFile(Node.Entry, GCMStream, Data))
                {
                    MessageBox.Show("File was successfully replaced.", "GCM", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("No space was found for the file. This shouldn't really happen unless a very large file was selected.", "GCM", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            RefreshPropertyGrid();
        }

        private void exportFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog f = new FolderBrowserDialog();

            if (f.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            using (FileStream GCMStream = OpenGCMStream())
            {
                if (treeView1.SelectedNode is GCMNode GCMNode)
                {
                    for (int i = 0; i < GCMNode.Nodes.Count; i++)
                    {
                        if (GCMNode.Nodes[i] is FolderNode FolderNode)
                        {
                            GCM.ExportDirectory(FolderNode, GCMStream, Path.Combine(f.SelectedPath, FolderNode.Entry.Name));
                        }
                        else if (GCMNode.Nodes[i] is FileNode FileNode)
                        {
                            using (FileStream Output = File.Open(Path.Combine(f.SelectedPath, FileNode.Entry.Name), FileMode.Create, FileAccess.Write))
                            {
                                GCM.ExportFile(FileNode.Entry, GCMStream, Output);
                            }
                        }
                    }
                }
                else if (treeView1.SelectedNode is FolderNode FolderNode)
                {
                    GCM.ExportDirectory(FolderNode, GCMStream, f.SelectedPath);
                }
            }
        }

        private void OpenGCM(string FileName)
        {
            GCMFilePath = FileName;

            // Read file
            using (Stream GCMStream = OpenGCMStream())
            {
                GCM = new GCM(GCMStream);
            }

            // Add to recent file list
            RecentFileList.AddFile(FileName);
            RecentFileList.SaveToDisk();

            // Create tree nodes
            GCMNode Root = GCM.CreateTreeNode(FileName);

            treeView1.Nodes.Clear();
            treeView1.Nodes.Add(Root);

            SetContextMenuStrip(Root);
        }
        private FileStream OpenGCMStream()
        {
            return File.Open(GCMFilePath, FileMode.Open, FileAccess.ReadWrite);
        }

        private void SetContextMenuStrip(TreeNode t)
        {
            switch (t)
            {
                case GCMNode GCM:
                case FolderNode FolderNode:
                    t.ContextMenuStrip = cmsFolder;
                    break;
                case FileNode FileNode:
                    t.ContextMenuStrip = cmsFile;
                    break;
                default:
                    throw new Exception();
            }

            for (int i = 0; i < t.Nodes.Count; i++)
            {
                SetContextMenuStrip(t.Nodes[i]);
            }
        }
        private void RefreshPropertyGrid()
        {
            propertyGrid1.SelectedObject = propertyGrid1.SelectedObject;
        }
    }
}
