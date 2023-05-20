using convertScr;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace scr2tapUI
{
    public partial class frmScr2Tap : Form
    {
        Dictionary<string, string> files = new Dictionary<string, string>();

        public frmScr2Tap()
        {
            InitializeComponent();
        }

        private void btnExplore_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();

            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                files.Clear();
                this.lswFiles.Clear();

                foreach (string file in Directory.GetFiles(dlg.SelectedPath, "*.scr"))
                {
                    string fileName = Path.GetFileName(file).Replace(Path.GetExtension(file), "");
                    files.Add(fileName, file);
                    this.lswFiles.Items.Add(fileName);
                }
            }
        }

        private void lswFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.btnConvert.Enabled = lswFiles.SelectedIndices.Count > 0;
        }

        private void btnConvert_Click(object sender, EventArgs e)
        {
            List<string> arguments = new List<string>();

            foreach(ListViewItem item in this.lswFiles.SelectedItems)
            {
                arguments.Add(files[item.Text]);
            }

            try
            {
                (new ConvertScr()).ToTap(arguments.ToArray());

                MessageBox.Show("Process success.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
    }
}
