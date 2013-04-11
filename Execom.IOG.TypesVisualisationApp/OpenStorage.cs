using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Execom.IOG.Storage;
using System.IO;

namespace Execom.IOG.TypesVisualisationApp
{
    public partial class OpenStorage : Form
    {
        private MainForm parent;

        public OpenStorage(MainForm parent)
        {
            this.parent = parent;

            InitializeComponent();
            lblStatus.Text = "";
            this.Height = 255;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                tbFilePath.Text = openFileDialog1.FileName;
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            disableControls();
            this.Cursor = Cursors.WaitCursor;
            Application.DoEvents();
            

            if (!ValidateForm())
                this.DialogResult = DialogResult.None;

            enableControls();
            this.Cursor = Cursors.Default;
            Application.DoEvents();
            
        }

        private bool ValidateForm()
        {
            bool isTextFieldsFilled = true;
            this.Height = 255;
            if (tbFilePath.Text.Equals(""))
            {
                lblFilePath.ForeColor = Color.Red;
                isTextFieldsFilled = false;
                lblStatus.Text = "Please fill in the required information.";
            }
            else
                lblFilePath.ForeColor = Color.Black;

            if (tbClusterSize.Text.Equals(""))
            {
                lblClusterSize.ForeColor = Color.Red;
                isTextFieldsFilled = false;
                lblStatus.Text = "Please fill in the required information.";
            }
            else
                lblClusterSize.ForeColor = Color.Black;
            if (!isTextFieldsFilled)
                return false;

            int clusterSize;
            if (!Int32.TryParse(tbClusterSize.Text, out clusterSize))
            {
                lblClusterSize.ForeColor = Color.Red;
                lblStatus.Text = "Cluster size must be an integer.";
                return false;
            }
            try
            {
                
                if(tbHeader.Text.Equals(""))
                    using ( FileStream file = new FileStream(tbFilePath.Text, FileMode.Open))
                    using (var storage = new IndexedFileStorage(file, clusterSize, checkSafeWrite.Checked))
                    {

                    }
                else
                    using (FileStream file = new FileStream(tbFilePath.Text, FileMode.Open))
                    using (var storage = new IndexedFileStorage(file, clusterSize, checkSafeWrite.Checked, tbHeader.Text))
                    {

                    }
            }
            catch (Exception e)
            {
                ExceptionDialog exDialog = new ExceptionDialog("An exception occured. \nPlease check the inputed parameters.",
                                                                e);
                
                exDialog.ShowDialog();
                return false;
            }

            if(tbHeader.Text.Equals(""))
                parent.setStorageInformation(tbFilePath.Text, clusterSize, checkSafeWrite.Checked);
            else
                parent.setStorageInformation(tbFilePath.Text, clusterSize, checkSafeWrite.Checked, tbHeader.Text);

            return true;
        }

        private void disableControls()
        {
            btnOk.Enabled = false;
            btnCancel.Enabled = false;
            btnBrowse.Enabled = false;
        }

        private void enableControls()
        {
            btnOk.Enabled = true;
            btnCancel.Enabled = true;
            btnBrowse.Enabled = true;
        }


    }
}
