using Editor.Format;
using System;
using System.IO;
using System.Windows.Forms;

namespace Editor
{
    public partial class Form1 : Form
    {
        private GCM GCM;
        private Stream GCMStream;

        public Form1()
        {
            InitializeComponent();
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
    }
}
