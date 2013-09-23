namespace Execom.IOG.TypesVisualisationApp
{
    partial class OpenStorage
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.lblFilePath = new System.Windows.Forms.Label();
            this.lblClusterSize = new System.Windows.Forms.Label();
            this.lblSafeWrite = new System.Windows.Forms.Label();
            this.lblHeader = new System.Windows.Forms.Label();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.tbFilePath = new System.Windows.Forms.TextBox();
            this.tbClusterSize = new System.Windows.Forms.TextBox();
            this.tbHeader = new System.Windows.Forms.TextBox();
            this.checkSafeWrite = new System.Windows.Forms.CheckBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.helpProvider1 = new System.Windows.Forms.HelpProvider();
            this.indexPanel = new System.Windows.Forms.Panel();
            this.mongoPanel = new System.Windows.Forms.Panel();
            this.tbCollectionName = new System.Windows.Forms.TextBox();
            this.lblColl = new System.Windows.Forms.Label();
            this.tbDatabaseName = new System.Windows.Forms.TextBox();
            this.lblDb = new System.Windows.Forms.Label();
            this.tbConnectionParams = new System.Windows.Forms.TextBox();
            this.lblConn = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.cbStorage = new System.Windows.Forms.ComboBox();
            this.cassPanel = new System.Windows.Forms.Panel();
            this.tbColumnFamily = new System.Windows.Forms.TextBox();
            this.lblColumnFamily = new System.Windows.Forms.Label();
            this.tbKeyspace = new System.Windows.Forms.TextBox();
            this.lblKeyspace = new System.Windows.Forms.Label();
            this.tbCluster = new System.Windows.Forms.TextBox();
            this.lblCluster = new System.Windows.Forms.Label();
            this.indexPanel.SuspendLayout();
            this.mongoPanel.SuspendLayout();
            this.cassPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblFilePath
            // 
            this.lblFilePath.AutoSize = true;
            this.lblFilePath.Location = new System.Drawing.Point(12, 15);
            this.lblFilePath.Name = "lblFilePath";
            this.lblFilePath.Size = new System.Drawing.Size(60, 13);
            this.lblFilePath.TabIndex = 0;
            this.lblFilePath.Text = "File path (*)";
            // 
            // lblClusterSize
            // 
            this.lblClusterSize.AutoSize = true;
            this.lblClusterSize.Location = new System.Drawing.Point(12, 70);
            this.lblClusterSize.Name = "lblClusterSize";
            this.lblClusterSize.Size = new System.Drawing.Size(75, 13);
            this.lblClusterSize.TabIndex = 1;
            this.lblClusterSize.Text = "Cluster Size (*)";
            // 
            // lblSafeWrite
            // 
            this.lblSafeWrite.AutoSize = true;
            this.lblSafeWrite.Location = new System.Drawing.Point(12, 120);
            this.lblSafeWrite.Name = "lblSafeWrite";
            this.lblSafeWrite.Size = new System.Drawing.Size(57, 13);
            this.lblSafeWrite.TabIndex = 2;
            this.lblSafeWrite.Text = "Safe Write";
            // 
            // lblHeader
            // 
            this.lblHeader.AutoSize = true;
            this.lblHeader.Location = new System.Drawing.Point(12, 96);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Size = new System.Drawing.Size(42, 13);
            this.lblHeader.TabIndex = 3;
            this.lblHeader.Text = "Header";
            // 
            // tbFilePath
            // 
            this.tbFilePath.Location = new System.Drawing.Point(119, 12);
            this.tbFilePath.Name = "tbFilePath";
            this.tbFilePath.Size = new System.Drawing.Size(233, 20);
            this.tbFilePath.TabIndex = 0;
            this.toolTip1.SetToolTip(this.tbFilePath, "Enter the path for\r\nthe storage file");
            // 
            // tbClusterSize
            // 
            this.tbClusterSize.Location = new System.Drawing.Point(119, 67);
            this.tbClusterSize.Name = "tbClusterSize";
            this.tbClusterSize.Size = new System.Drawing.Size(44, 20);
            this.tbClusterSize.TabIndex = 2;
            this.toolTip1.SetToolTip(this.tbClusterSize, "Enter storage file cluster size ");
            // 
            // tbHeader
            // 
            this.tbHeader.Location = new System.Drawing.Point(119, 93);
            this.tbHeader.Name = "tbHeader";
            this.tbHeader.Size = new System.Drawing.Size(67, 20);
            this.tbHeader.TabIndex = 3;
            this.toolTip1.SetToolTip(this.tbHeader, "Enter the storage file header");
            // 
            // checkSafeWrite
            // 
            this.checkSafeWrite.AutoSize = true;
            this.checkSafeWrite.Checked = true;
            this.checkSafeWrite.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkSafeWrite.Location = new System.Drawing.Point(119, 120);
            this.checkSafeWrite.Name = "checkSafeWrite";
            this.checkSafeWrite.Size = new System.Drawing.Size(15, 14);
            this.checkSafeWrite.TabIndex = 4;
            this.toolTip1.SetToolTip(this.checkSafeWrite, "Safe write");
            this.checkSafeWrite.UseVisualStyleBackColor = true;
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(119, 38);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnBrowse.TabIndex = 1;
            this.btnBrowse.Text = "Browse";
            this.toolTip1.SetToolTip(this.btnBrowse, "Browse for storage file");
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStatus.ForeColor = System.Drawing.Color.Black;
            this.lblStatus.Location = new System.Drawing.Point(12, 142);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(47, 13);
            this.lblStatus.TabIndex = 9;
            this.lblStatus.Text = "lblStatus";
            // 
            // btnOk
            // 
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Location = new System.Drawing.Point(206, 201);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 5;
            this.btnOk.Text = "Ok";
            this.toolTip1.SetToolTip(this.btnOk, "Open storage file");
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(287, 201);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 6;
            this.btnCancel.Text = "Cancel";
            this.toolTip1.SetToolTip(this.btnCancel, "Cancel");
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 214);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(90, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "Press \'F1\' for help";
            // 
            // helpProvider1
            // 
            this.helpProvider1.HelpNamespace = "..\\..\\help\\help.chm";
            // 
            // indexPanel
            // 
            this.indexPanel.Controls.Add(this.lblFilePath);
            this.indexPanel.Controls.Add(this.lblClusterSize);
            this.indexPanel.Controls.Add(this.lblSafeWrite);
            this.indexPanel.Controls.Add(this.lblHeader);
            this.indexPanel.Controls.Add(this.lblStatus);
            this.indexPanel.Controls.Add(this.tbFilePath);
            this.indexPanel.Controls.Add(this.btnBrowse);
            this.indexPanel.Controls.Add(this.tbClusterSize);
            this.indexPanel.Controls.Add(this.checkSafeWrite);
            this.indexPanel.Controls.Add(this.tbHeader);
            this.indexPanel.Location = new System.Drawing.Point(10, 33);
            this.indexPanel.Name = "indexPanel";
            this.indexPanel.Size = new System.Drawing.Size(363, 162);
            this.indexPanel.TabIndex = 11;
            // 
            // mongoPanel
            // 
            this.mongoPanel.Controls.Add(this.tbCollectionName);
            this.mongoPanel.Controls.Add(this.lblColl);
            this.mongoPanel.Controls.Add(this.tbDatabaseName);
            this.mongoPanel.Controls.Add(this.lblDb);
            this.mongoPanel.Controls.Add(this.tbConnectionParams);
            this.mongoPanel.Controls.Add(this.lblConn);
            this.mongoPanel.Location = new System.Drawing.Point(10, 33);
            this.mongoPanel.Name = "mongoPanel";
            this.mongoPanel.Size = new System.Drawing.Size(363, 162);
            this.mongoPanel.TabIndex = 12;
            this.mongoPanel.Visible = false;
            // 
            // tbCollectionName
            // 
            this.tbCollectionName.Location = new System.Drawing.Point(119, 106);
            this.tbCollectionName.Name = "tbCollectionName";
            this.tbCollectionName.Size = new System.Drawing.Size(233, 20);
            this.tbCollectionName.TabIndex = 14;
            this.tbCollectionName.Text = "test";
            this.toolTip1.SetToolTip(this.tbCollectionName, "Collection name");
            // 
            // lblColl
            // 
            this.lblColl.AutoSize = true;
            this.lblColl.Location = new System.Drawing.Point(12, 109);
            this.lblColl.Name = "lblColl";
            this.lblColl.Size = new System.Drawing.Size(56, 13);
            this.lblColl.TabIndex = 13;
            this.lblColl.Text = "Collection:";
            // 
            // tbDatabaseName
            // 
            this.tbDatabaseName.Location = new System.Drawing.Point(119, 63);
            this.tbDatabaseName.Name = "tbDatabaseName";
            this.tbDatabaseName.Size = new System.Drawing.Size(233, 20);
            this.tbDatabaseName.TabIndex = 12;
            this.tbDatabaseName.Text = "test";
            this.toolTip1.SetToolTip(this.tbDatabaseName, "Database name");
            // 
            // lblDb
            // 
            this.lblDb.AutoSize = true;
            this.lblDb.Location = new System.Drawing.Point(12, 66);
            this.lblDb.Name = "lblDb";
            this.lblDb.Size = new System.Drawing.Size(56, 13);
            this.lblDb.TabIndex = 11;
            this.lblDb.Text = "Database:";
            // 
            // tbConnectionParams
            // 
            this.tbConnectionParams.Location = new System.Drawing.Point(119, 18);
            this.tbConnectionParams.Name = "tbConnectionParams";
            this.tbConnectionParams.Size = new System.Drawing.Size(233, 20);
            this.tbConnectionParams.TabIndex = 10;
            this.tbConnectionParams.Text = "mongodb://ws014:27017/test";
            this.toolTip1.SetToolTip(this.tbConnectionParams, "Input connection parameters");
            // 
            // lblConn
            // 
            this.lblConn.AutoSize = true;
            this.lblConn.Location = new System.Drawing.Point(12, 21);
            this.lblConn.Name = "lblConn";
            this.lblConn.Size = new System.Drawing.Size(64, 13);
            this.lblConn.TabIndex = 9;
            this.lblConn.Text = "Connection:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(22, 9);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(47, 13);
            this.label5.TabIndex = 15;
            this.label5.Text = "Storage:";
            // 
            // cbStorage
            // 
            this.cbStorage.FormattingEnabled = true;
            this.cbStorage.Items.AddRange(new object[] {
            "Indexed file",
            "MongoDB",
            "Cassandra"});
            this.cbStorage.Location = new System.Drawing.Point(129, 6);
            this.cbStorage.Name = "cbStorage";
            this.cbStorage.Size = new System.Drawing.Size(121, 21);
            this.cbStorage.TabIndex = 16;
            this.cbStorage.Text = "Indexed file";
            this.cbStorage.SelectedIndexChanged += new System.EventHandler(this.cbStorage_SelectedIndexChanged);
            // 
            // cassPanel
            // 
            this.cassPanel.Controls.Add(this.tbColumnFamily);
            this.cassPanel.Controls.Add(this.lblColumnFamily);
            this.cassPanel.Controls.Add(this.tbKeyspace);
            this.cassPanel.Controls.Add(this.lblKeyspace);
            this.cassPanel.Controls.Add(this.tbCluster);
            this.cassPanel.Controls.Add(this.lblCluster);
            this.cassPanel.Location = new System.Drawing.Point(10, 33);
            this.cassPanel.Name = "cassPanel";
            this.cassPanel.Size = new System.Drawing.Size(363, 162);
            this.cassPanel.TabIndex = 15;
            this.cassPanel.Visible = false;
            // 
            // tbColumnFamily
            // 
            this.tbColumnFamily.Location = new System.Drawing.Point(119, 106);
            this.tbColumnFamily.Name = "tbColumnFamily";
            this.tbColumnFamily.Size = new System.Drawing.Size(233, 20);
            this.tbColumnFamily.TabIndex = 14;
            this.tbColumnFamily.Text = "IOGFamily";
            this.toolTip1.SetToolTip(this.tbColumnFamily, "Column family name");
            // 
            // lblColumnFamily
            // 
            this.lblColumnFamily.AutoSize = true;
            this.lblColumnFamily.Location = new System.Drawing.Point(12, 109);
            this.lblColumnFamily.Name = "lblColumnFamily";
            this.lblColumnFamily.Size = new System.Drawing.Size(74, 13);
            this.lblColumnFamily.TabIndex = 13;
            this.lblColumnFamily.Text = "Column family:";
            // 
            // tbKeyspace
            // 
            this.tbKeyspace.Location = new System.Drawing.Point(119, 63);
            this.tbKeyspace.Name = "tbKeyspace";
            this.tbKeyspace.Size = new System.Drawing.Size(233, 20);
            this.tbKeyspace.TabIndex = 12;
            this.tbKeyspace.Text = "IOGKeyspace";
            this.toolTip1.SetToolTip(this.tbKeyspace, "Keyspace name");
            // 
            // lblKeyspace
            // 
            this.lblKeyspace.AutoSize = true;
            this.lblKeyspace.Location = new System.Drawing.Point(12, 66);
            this.lblKeyspace.Name = "lblKeyspace";
            this.lblKeyspace.Size = new System.Drawing.Size(57, 13);
            this.lblKeyspace.TabIndex = 11;
            this.lblKeyspace.Text = "Keyspace:";
            // 
            // tbCluster
            // 
            this.tbCluster.Location = new System.Drawing.Point(119, 18);
            this.tbCluster.Name = "tbCluster";
            this.tbCluster.Size = new System.Drawing.Size(233, 20);
            this.tbCluster.TabIndex = 10;
            this.tbCluster.Text = "Test Cluster";
            this.toolTip1.SetToolTip(this.tbCluster, "Cluster name");
            // 
            // lblCluster
            // 
            this.lblCluster.AutoSize = true;
            this.lblCluster.Location = new System.Drawing.Point(12, 21);
            this.lblCluster.Name = "lblCluster";
            this.lblCluster.Size = new System.Drawing.Size(42, 13);
            this.lblCluster.TabIndex = 9;
            this.lblCluster.Text = "Cluster:";
            // 
            // OpenStorage
            // 
            this.AcceptButton = this.btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(385, 245);
            this.Controls.Add(this.cassPanel);
            this.Controls.Add(this.cbStorage);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.mongoPanel);
            this.Controls.Add(this.indexPanel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.helpProvider1.SetHelpKeyword(this, "Opening indexed file storage");
            this.helpProvider1.SetHelpNavigator(this, System.Windows.Forms.HelpNavigator.KeywordIndex);
            this.Name = "OpenStorage";
            this.helpProvider1.SetShowHelp(this, true);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Open Storage";
            this.indexPanel.ResumeLayout(false);
            this.indexPanel.PerformLayout();
            this.mongoPanel.ResumeLayout(false);
            this.mongoPanel.PerformLayout();
            this.cassPanel.ResumeLayout(false);
            this.cassPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblFilePath;
        private System.Windows.Forms.Label lblClusterSize;
        private System.Windows.Forms.Label lblSafeWrite;
        private System.Windows.Forms.Label lblHeader;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.TextBox tbFilePath;
        private System.Windows.Forms.TextBox tbClusterSize;
        private System.Windows.Forms.TextBox tbHeader;
        private System.Windows.Forms.CheckBox checkSafeWrite;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.HelpProvider helpProvider1;
        private System.Windows.Forms.Panel indexPanel;
        private System.Windows.Forms.Panel mongoPanel;
        private System.Windows.Forms.TextBox tbCollectionName;
        private System.Windows.Forms.Label lblColl;
        private System.Windows.Forms.TextBox tbDatabaseName;
        private System.Windows.Forms.Label lblDb;
        private System.Windows.Forms.TextBox tbConnectionParams;
        private System.Windows.Forms.Label lblConn;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cbStorage;
        private System.Windows.Forms.Panel cassPanel;
        private System.Windows.Forms.TextBox tbColumnFamily;
        private System.Windows.Forms.Label lblColumnFamily;
        private System.Windows.Forms.TextBox tbKeyspace;
        private System.Windows.Forms.Label lblKeyspace;
        private System.Windows.Forms.TextBox tbCluster;
        private System.Windows.Forms.Label lblCluster;
    }
}