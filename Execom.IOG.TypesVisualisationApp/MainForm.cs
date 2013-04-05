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
    public partial class MainForm : Form
    {
        private static readonly string defaultHeaderValue = "IdxFs";

        private string filePath;
        private int clusterSize;
        private bool safeWrite;
        private string header;

        public MainForm()
        {
            InitializeComponent();
            gbCurrentStorage.Visible = false;
            btnCloseStorage.Enabled = false;
            lblNoStorage.Visible = true;
            tbStatus.Text = "";
            panStatus.Visible = false;
        }

        public void setStorageInformation(string filePath, int clusterSize, bool safeWrite, string header)
        {
            this.filePath = filePath;
            this.clusterSize = clusterSize;
            this.safeWrite = safeWrite;
            this.header = header;
            tbFilePath.Text = this.filePath;
            tbClusterSize.Text = this.clusterSize.ToString();
            cbSafeWrite.Checked = safeWrite;
            tbHeader.Text = header;
        }

        public void setStorageInformation(string filePath, int clusterSize, bool safeWrite)
        {
            this.filePath = filePath;
            this.clusterSize = clusterSize;
            this.safeWrite = safeWrite;
            this.header = defaultHeaderValue;
            tbFilePath.Text = this.filePath;
            tbClusterSize.Text = this.clusterSize.ToString();
            cbSafeWrite.Checked = safeWrite;
            tbHeader.Text = this.header;
        }

        private void btnOpenStorage_Click(object sender, EventArgs e)
        {

            var dialog = new OpenStorage(this);
            if (dialog.ShowDialog() == DialogResult.OK && filePath != null)
            {
                gbCurrentStorage.Visible = true;
                lblNoStorage.Visible = false;
                btnCloseStorage.Enabled = true;
                btnOpenStorage.Enabled = false;
                tbStatus.Text = "";
                panStatus.Visible = false;
            }
        }

        private void btnCloseStorage_Click(object sender, EventArgs e)
        {
            gbCurrentStorage.Visible = false;
            lblNoStorage.Visible = true;
            btnCloseStorage.Enabled = false;
            btnOpenStorage.Enabled = true;
            this.filePath = null;
            tbStatus.Text = "";
            panStatus.Visible = false;
        }

        private void btnCreateGVFile_Click(object sender, EventArgs e)
        {
            
            try
            {
                string gvContent = null;
                using (FileStream file = new FileStream(filePath, FileMode.Open))
                using (var storage = new IndexedFileStorage(file, clusterSize, safeWrite, header))
                {
                    
                    if (rbRootType.Checked)
                    {
                        gvContent = Context.GetGraphVizContentFromStorage(storage);
                        
                    }
                    else
                    {
                        ChooseTypeForm chooseTypeForm = new ChooseTypeForm(Context.GetTypeVisualisationUnitsFromStorage(storage));
                        if (chooseTypeForm.ShowDialog() == DialogResult.OK)
                        {
                            gvContent = Context.GetGraphVizContentFromStorage(chooseTypeForm.CurrentType.Name, storage);
                        }
                    }
                }
                if (gvContent != null)
                {
                    saveFileDialog1.Filter = "GraphViz file|*.gv";
                    saveFileDialog1.AddExtension = true;
                    saveFileDialog1.DefaultExt = ".gv";
                    if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        using (StreamWriter sw = new StreamWriter(saveFileDialog1.FileName))
                            sw.Write(gvContent);
                        panStatus.Visible = true;
                        tbStatus.Text = saveFileDialog1.FileName;
                    }
                }
                
            }
            catch (FileNotFoundException ex)
            {
                String message = "File Path is invalid! The file has probably been moved or removed since opening it.\n Please check the file location.";
                ExceptionDialog exDialog = new ExceptionDialog(message, ex);
                exDialog.ShowDialog();
            }
            catch (Exception ex)
            {
                String message = "An exception has occured. Please consult the exception log below.";
                ExceptionDialog exDialog = new ExceptionDialog(message, ex);
                exDialog.ShowDialog();
            }

            }

        }
    
}
