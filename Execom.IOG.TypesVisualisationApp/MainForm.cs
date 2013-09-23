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
using GraphVizWrapper.Queries;
using GraphVizWrapper.Commands;
using GraphVizWrapper;
using System.Diagnostics;


namespace Execom.IOG.TypesVisualisationApp
{
    public partial class MainForm : Form
    {
        private static readonly string defaultHeaderValue = "IdxFs";

        // Indexed file storage parameters
        private string filePath;
        private int clusterSize;
        private bool safeWrite;
        private string header;

        // Mongo storage parameters
        private string connParams;
        private string collParams;
        private string dbParams;
        
        // Aquiles storage parameters
        private string cluster;
        private string keyspace;
        private string columnFamily;

        private IKeyValueStorage<Guid, object> storage;
        public string storageType = "index";

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

        public void setMongoStorageInformation(string connParams, string dbParams, string collParams)
        {
            this.connParams = connParams;
            this.dbParams = dbParams;
            this.collParams = collParams;
            tbConn.Text = connParams;
            tbDb.Text = dbParams;
            tbColl.Text = collParams;            
        }

        public void setAquilesStorageInformation(string cluster, string keyspace, string columnFamily)
        {
            this.cluster = cluster;
            this.keyspace = keyspace;
            this.columnFamily = columnFamily;
            tbCluster.Text = cluster;
            tbKeyspace.Text = keyspace;
            tbColumnFamily.Text = columnFamily;
        }

        private void btnOpenStorage_Click(object sender, EventArgs e)
        {            
            var dialog = new OpenStorage(this);
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                gbCurrentStorage.Visible = true;
                if (storageType.Equals("index"))
                {
                    mongoPanel.Visible = false;
                    indexPanel.Visible = true;
                    cassPanel.Visible = false;
                }
                if (storageType.Equals("mongo"))
                {
                    indexPanel.Visible = false;
                    mongoPanel.Visible = true;
                    cassPanel.Visible = false;
                } 
                if (storageType.Equals("cassandra"))
                {
                    indexPanel.Visible = false;
                    mongoPanel.Visible = false;
                    cassPanel.Visible = true;
                }
                lblNoStorage.Visible = false;
                btnCloseStorage.Enabled = true;
                btnOpenStorage.Enabled = false;
                tbStatus.Text = "";
                panStatus.Visible = false;
                helpProvider1.SetHelpKeyword(this, "Visualization");
            }

        }

        private void btnCloseStorage_Click(object sender, EventArgs e)
        {
            gbCurrentStorage.Visible = false;
            lblNoStorage.Visible = true;
            btnCloseStorage.Enabled = false;
            btnOpenStorage.Enabled = true;
            storageType = "index";
            tbStatus.Text = "";
            panStatus.Visible = false;
        }

        private void btnCreateGVFile_Click(object sender, EventArgs e)
        {
            disableControls();
            this.Cursor = Cursors.WaitCursor;
            Application.DoEvents();

            try
            {
                string gvContent = null;
                string chosenTypeName = null;
                //Opening the storage
                if (storageType.Equals("index"))
                {
                    FileStream file = new FileStream(filePath, FileMode.Open);
                    storage = new IndexedFileStorage(file, clusterSize, safeWrite, header);
                }
                if (storageType.Equals("mongo"))
                {
                    storage = new MongoStorage.MongoStorage(connParams, dbParams, collParams);
                }
                if (storageType.Equals("cassandra"))
                {
                    storage = new AquilesStorage.AquilesStorage(cluster, keyspace, columnFamily);
                }

                string rootTypeName = Context.GetRootTypeNameFromStorage(storage);

                if (rbRootType.Checked)
                {
                    chosenTypeName = rootTypeName;
                    gvContent = Context.GetGraphVizContentFromStorage(storage);
                }
                else
                {
                    //Opening the dialog for choosing a type.
                    ChooseTypeForm chooseTypeForm = new ChooseTypeForm(Context.GetTypeVisualisationUnitsFromStorage(storage), rootTypeName);
                    if (chooseTypeForm.ShowDialog() == DialogResult.OK)
                    {
                        chosenTypeName = chooseTypeForm.CurrentType.Name;
                        gvContent = Context.GetGraphVizContentFromStorage(chosenTypeName, storage);
                    }
                }

                if (gvContent != null)
                {
                    this.Cursor = Cursors.WaitCursor;
                    Application.DoEvents();

                    //creating the filename for the image.
                    filePath = "..\\..\\img\\";
                    string fileLocation = filePath.Substring(0, filePath.LastIndexOf("\\") + 1);
                    string fileName = Path.GetFileNameWithoutExtension(filePath) + "_" + chosenTypeName;
                    string pngExtension = ".png";
                    string newFilePathPng = fileLocation + fileName + pngExtension;
                    int occurance = 0;
                    while (File.Exists(newFilePathPng))
                    {
                        occurance++;
                        newFilePathPng = fileLocation + fileName + occurance + pngExtension;
                    }

                    //Calling dot.exe to generate the image.
                    var getStartProcessQuery = new GetStartProcessQuery();
                    var getProcessStartInfoQuery = new GetProcessStartInfoQuery();
                    var registerLayoutPluginCommand = new RegisterLayoutPluginCommand(getProcessStartInfoQuery, getStartProcessQuery);
                    var wrapper = new GraphVizWrapper.GraphVizWrapper(getStartProcessQuery, getProcessStartInfoQuery, registerLayoutPluginCommand);

                    byte[] output = wrapper.GenerateGraph(gvContent, Enums.GraphReturnType.Png);
                    File.WriteAllBytes(newFilePathPng, output);

                    panStatus.Visible = true;
                    tbStatus.Text = newFilePathPng;
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
            enableControls();
            this.Cursor = Cursors.Default;
            Application.DoEvents();

        }

        private void btnShow_Click(object sender, EventArgs e)
        {
            if (!(tbStatus.Text.Equals("")))
                Process.Start(tbStatus.Text);


        }

        private void disableControls()
        {
            btnCreateGVFile.Enabled = false;
            rbRootType.Enabled = false;
            rbSpecificType.Enabled = false;
            panStatus.Enabled = false;
            btnCloseStorage.Enabled = false;
            lblFilePath.Focus();
        }

        private void enableControls()
        {
            btnCreateGVFile.Enabled = true;
            rbRootType.Enabled = true;
            rbSpecificType.Enabled = true;
            panStatus.Enabled = true;
            btnCloseStorage.Enabled = true;

        }
    }
    
}
