namespace Execom.IOG.TypesVisualisationApp
{
    partial class MainForm
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
            this.btnOpenStorage = new System.Windows.Forms.Button();
            this.gbCurrentStorage = new System.Windows.Forms.GroupBox();
            this.mongoPanel = new System.Windows.Forms.Panel();
            this.tbColl = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.tbDb = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.tbConn = new System.Windows.Forms.TextBox();
            this.indexPanel = new System.Windows.Forms.Panel();
            this.tbHeader = new System.Windows.Forms.TextBox();
            this.lblFilePath = new System.Windows.Forms.Label();
            this.cbSafeWrite = new System.Windows.Forms.CheckBox();
            this.lblClusterSize = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.tbClusterSize = new System.Windows.Forms.TextBox();
            this.lblHeader = new System.Windows.Forms.Label();
            this.tbFilePath = new System.Windows.Forms.TextBox();
            this.gbCreateImageFile = new System.Windows.Forms.GroupBox();
            this.btnCreateGVFile = new System.Windows.Forms.Button();
            this.rbSpecificType = new System.Windows.Forms.RadioButton();
            this.rbRootType = new System.Windows.Forms.RadioButton();
            this.lblNoStorage = new System.Windows.Forms.Label();
            this.btnCloseStorage = new System.Windows.Forms.Button();
            this.tbStatus = new System.Windows.Forms.TextBox();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.lblStatus = new System.Windows.Forms.Label();
            this.panStatus = new System.Windows.Forms.Panel();
            this.btnShow = new System.Windows.Forms.Button();
            this.helpProvider1 = new System.Windows.Forms.HelpProvider();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.cassPanel = new System.Windows.Forms.Panel();
            this.tbColumnFamily = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.tbKeyspace = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.tbCluster = new System.Windows.Forms.TextBox();
            this.gbCurrentStorage.SuspendLayout();
            this.mongoPanel.SuspendLayout();
            this.indexPanel.SuspendLayout();
            this.gbCreateImageFile.SuspendLayout();
            this.panStatus.SuspendLayout();
            this.cassPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOpenStorage
            // 
            this.btnOpenStorage.Location = new System.Drawing.Point(34, 23);
            this.btnOpenStorage.Name = "btnOpenStorage";
            this.btnOpenStorage.Size = new System.Drawing.Size(72, 36);
            this.btnOpenStorage.TabIndex = 0;
            this.btnOpenStorage.Text = "Open Storage File";
            this.toolTip1.SetToolTip(this.btnOpenStorage, "Select storage file");
            this.btnOpenStorage.UseVisualStyleBackColor = true;
            this.btnOpenStorage.Click += new System.EventHandler(this.btnOpenStorage_Click);
            // 
            // gbCurrentStorage
            // 
            this.gbCurrentStorage.Controls.Add(this.cassPanel);
            this.gbCurrentStorage.Controls.Add(this.mongoPanel);
            this.gbCurrentStorage.Controls.Add(this.indexPanel);
            this.gbCurrentStorage.Controls.Add(this.gbCreateImageFile);
            this.gbCurrentStorage.Location = new System.Drawing.Point(27, 75);
            this.gbCurrentStorage.Name = "gbCurrentStorage";
            this.gbCurrentStorage.Size = new System.Drawing.Size(343, 200);
            this.gbCurrentStorage.TabIndex = 1;
            this.gbCurrentStorage.TabStop = false;
            this.gbCurrentStorage.Text = "Current storage opened";
            // 
            // mongoPanel
            // 
            this.mongoPanel.Controls.Add(this.tbColl);
            this.mongoPanel.Controls.Add(this.label2);
            this.mongoPanel.Controls.Add(this.label4);
            this.mongoPanel.Controls.Add(this.tbDb);
            this.mongoPanel.Controls.Add(this.label6);
            this.mongoPanel.Controls.Add(this.tbConn);
            this.mongoPanel.Location = new System.Drawing.Point(6, 19);
            this.mongoPanel.Name = "mongoPanel";
            this.mongoPanel.Size = new System.Drawing.Size(324, 98);
            this.mongoPanel.TabIndex = 8;
            this.mongoPanel.Visible = false;
            // 
            // tbColl
            // 
            this.tbColl.Location = new System.Drawing.Point(76, 67);
            this.tbColl.Name = "tbColl";
            this.tbColl.ReadOnly = true;
            this.tbColl.Size = new System.Drawing.Size(230, 20);
            this.tbColl.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(5, 7);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(64, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Connection:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(5, 37);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(56, 13);
            this.label4.TabIndex = 1;
            this.label4.Text = "Database:";
            // 
            // tbDb
            // 
            this.tbDb.Location = new System.Drawing.Point(76, 34);
            this.tbDb.Name = "tbDb";
            this.tbDb.ReadOnly = true;
            this.tbDb.Size = new System.Drawing.Size(230, 20);
            this.tbDb.TabIndex = 5;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(5, 70);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(56, 13);
            this.label6.TabIndex = 3;
            this.label6.Text = "Collection:";
            // 
            // tbConn
            // 
            this.tbConn.Location = new System.Drawing.Point(76, 4);
            this.tbConn.Name = "tbConn";
            this.tbConn.ReadOnly = true;
            this.tbConn.Size = new System.Drawing.Size(230, 20);
            this.tbConn.TabIndex = 4;
            // 
            // indexPanel
            // 
            this.indexPanel.Controls.Add(this.tbHeader);
            this.indexPanel.Controls.Add(this.lblFilePath);
            this.indexPanel.Controls.Add(this.cbSafeWrite);
            this.indexPanel.Controls.Add(this.lblClusterSize);
            this.indexPanel.Controls.Add(this.label3);
            this.indexPanel.Controls.Add(this.tbClusterSize);
            this.indexPanel.Controls.Add(this.lblHeader);
            this.indexPanel.Controls.Add(this.tbFilePath);
            this.indexPanel.Location = new System.Drawing.Point(6, 19);
            this.indexPanel.Name = "indexPanel";
            this.indexPanel.Size = new System.Drawing.Size(324, 98);
            this.indexPanel.TabIndex = 7;
            // 
            // tbHeader
            // 
            this.tbHeader.Location = new System.Drawing.Point(79, 55);
            this.tbHeader.Name = "tbHeader";
            this.tbHeader.ReadOnly = true;
            this.tbHeader.Size = new System.Drawing.Size(78, 20);
            this.tbHeader.TabIndex = 6;
            // 
            // lblFilePath
            // 
            this.lblFilePath.AutoSize = true;
            this.lblFilePath.Location = new System.Drawing.Point(8, 7);
            this.lblFilePath.Name = "lblFilePath";
            this.lblFilePath.Size = new System.Drawing.Size(50, 13);
            this.lblFilePath.TabIndex = 0;
            this.lblFilePath.Text = "File path:";
            // 
            // cbSafeWrite
            // 
            this.cbSafeWrite.AutoSize = true;
            this.cbSafeWrite.Enabled = false;
            this.cbSafeWrite.Location = new System.Drawing.Point(79, 82);
            this.cbSafeWrite.Name = "cbSafeWrite";
            this.cbSafeWrite.Size = new System.Drawing.Size(15, 14);
            this.cbSafeWrite.TabIndex = 7;
            this.cbSafeWrite.UseVisualStyleBackColor = true;
            // 
            // lblClusterSize
            // 
            this.lblClusterSize.AutoSize = true;
            this.lblClusterSize.Location = new System.Drawing.Point(8, 33);
            this.lblClusterSize.Name = "lblClusterSize";
            this.lblClusterSize.Size = new System.Drawing.Size(65, 13);
            this.lblClusterSize.TabIndex = 1;
            this.lblClusterSize.Text = "Cluster Size:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 83);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(60, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Safe Write:";
            // 
            // tbClusterSize
            // 
            this.tbClusterSize.Location = new System.Drawing.Point(79, 30);
            this.tbClusterSize.Name = "tbClusterSize";
            this.tbClusterSize.ReadOnly = true;
            this.tbClusterSize.Size = new System.Drawing.Size(46, 20);
            this.tbClusterSize.TabIndex = 5;
            // 
            // lblHeader
            // 
            this.lblHeader.AutoSize = true;
            this.lblHeader.Location = new System.Drawing.Point(8, 58);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Size = new System.Drawing.Size(45, 13);
            this.lblHeader.TabIndex = 3;
            this.lblHeader.Text = "Header:";
            // 
            // tbFilePath
            // 
            this.tbFilePath.Location = new System.Drawing.Point(79, 4);
            this.tbFilePath.Name = "tbFilePath";
            this.tbFilePath.ReadOnly = true;
            this.tbFilePath.Size = new System.Drawing.Size(230, 20);
            this.tbFilePath.TabIndex = 4;
            // 
            // gbCreateImageFile
            // 
            this.gbCreateImageFile.Controls.Add(this.btnCreateGVFile);
            this.gbCreateImageFile.Controls.Add(this.rbSpecificType);
            this.gbCreateImageFile.Controls.Add(this.rbRootType);
            this.gbCreateImageFile.Location = new System.Drawing.Point(7, 120);
            this.gbCreateImageFile.Name = "gbCreateImageFile";
            this.gbCreateImageFile.Size = new System.Drawing.Size(330, 67);
            this.gbCreateImageFile.TabIndex = 12;
            this.gbCreateImageFile.TabStop = false;
            this.gbCreateImageFile.Text = "Create Image File";
            // 
            // btnCreateGVFile
            // 
            this.btnCreateGVFile.Location = new System.Drawing.Point(234, 27);
            this.btnCreateGVFile.Name = "btnCreateGVFile";
            this.btnCreateGVFile.Size = new System.Drawing.Size(75, 23);
            this.btnCreateGVFile.TabIndex = 4;
            this.btnCreateGVFile.Text = "Ok";
            this.toolTip1.SetToolTip(this.btnCreateGVFile, "Create image");
            this.btnCreateGVFile.UseVisualStyleBackColor = true;
            this.btnCreateGVFile.Click += new System.EventHandler(this.btnCreateGVFile_Click);
            // 
            // rbSpecificType
            // 
            this.rbSpecificType.AutoSize = true;
            this.rbSpecificType.Location = new System.Drawing.Point(115, 30);
            this.rbSpecificType.Name = "rbSpecificType";
            this.rbSpecificType.Size = new System.Drawing.Size(113, 17);
            this.rbSpecificType.TabIndex = 3;
            this.rbSpecificType.Text = "from Specific Type";
            this.toolTip1.SetToolTip(this.rbSpecificType, "Create image using a specific type as the graph root");
            this.rbSpecificType.UseVisualStyleBackColor = true;
            // 
            // rbRootType
            // 
            this.rbRootType.AutoSize = true;
            this.rbRootType.Checked = true;
            this.rbRootType.Location = new System.Drawing.Point(11, 30);
            this.rbRootType.Name = "rbRootType";
            this.rbRootType.Size = new System.Drawing.Size(98, 17);
            this.rbRootType.TabIndex = 2;
            this.rbRootType.TabStop = true;
            this.rbRootType.Text = "from Root Type";
            this.toolTip1.SetToolTip(this.rbRootType, "Create image using root type as the graph root");
            this.rbRootType.UseVisualStyleBackColor = true;
            // 
            // lblNoStorage
            // 
            this.lblNoStorage.AutoSize = true;
            this.lblNoStorage.Location = new System.Drawing.Point(31, 75);
            this.lblNoStorage.Name = "lblNoStorage";
            this.lblNoStorage.Size = new System.Drawing.Size(154, 13);
            this.lblNoStorage.TabIndex = 2;
            this.lblNoStorage.Text = "No storage is currently opened.";
            // 
            // btnCloseStorage
            // 
            this.btnCloseStorage.Location = new System.Drawing.Point(120, 23);
            this.btnCloseStorage.Name = "btnCloseStorage";
            this.btnCloseStorage.Size = new System.Drawing.Size(72, 36);
            this.btnCloseStorage.TabIndex = 1;
            this.btnCloseStorage.Text = "Close Storage File";
            this.toolTip1.SetToolTip(this.btnCloseStorage, "Close storage file");
            this.btnCloseStorage.UseVisualStyleBackColor = true;
            this.btnCloseStorage.Click += new System.EventHandler(this.btnCloseStorage_Click);
            // 
            // tbStatus
            // 
            this.tbStatus.Location = new System.Drawing.Point(7, 19);
            this.tbStatus.Name = "tbStatus";
            this.tbStatus.ReadOnly = true;
            this.tbStatus.Size = new System.Drawing.Size(228, 20);
            this.tbStatus.TabIndex = 3;
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(4, 3);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(144, 13);
            this.lblStatus.TabIndex = 4;
            this.lblStatus.Text = "Image file has been created: ";
            // 
            // panStatus
            // 
            this.panStatus.Controls.Add(this.btnShow);
            this.panStatus.Controls.Add(this.lblStatus);
            this.panStatus.Controls.Add(this.tbStatus);
            this.panStatus.Location = new System.Drawing.Point(27, 281);
            this.panStatus.Name = "panStatus";
            this.panStatus.Size = new System.Drawing.Size(360, 44);
            this.panStatus.TabIndex = 5;
            this.panStatus.Visible = false;
            // 
            // btnShow
            // 
            this.btnShow.Location = new System.Drawing.Point(241, 16);
            this.btnShow.Name = "btnShow";
            this.btnShow.Size = new System.Drawing.Size(75, 23);
            this.btnShow.TabIndex = 5;
            this.btnShow.Text = "Show Image";
            this.toolTip1.SetToolTip(this.btnShow, "Show created image");
            this.btnShow.UseVisualStyleBackColor = true;
            this.btnShow.Click += new System.EventHandler(this.btnShow_Click);
            // 
            // helpProvider1
            // 
            this.helpProvider1.HelpNamespace = "..\\..\\help\\help.chm";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 341);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(90, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Press \'F1\' for help";
            // 
            // cassPanel
            // 
            this.cassPanel.Controls.Add(this.tbColumnFamily);
            this.cassPanel.Controls.Add(this.label5);
            this.cassPanel.Controls.Add(this.label7);
            this.cassPanel.Controls.Add(this.tbKeyspace);
            this.cassPanel.Controls.Add(this.label8);
            this.cassPanel.Controls.Add(this.tbCluster);
            this.cassPanel.Location = new System.Drawing.Point(6, 19);
            this.cassPanel.Name = "cassPanel";
            this.cassPanel.Size = new System.Drawing.Size(324, 98);
            this.cassPanel.TabIndex = 9;
            this.cassPanel.Visible = false;
            // 
            // tbColumnFamily
            // 
            this.tbColumnFamily.Location = new System.Drawing.Point(76, 67);
            this.tbColumnFamily.Name = "tbColumnFamily";
            this.tbColumnFamily.ReadOnly = true;
            this.tbColumnFamily.Size = new System.Drawing.Size(230, 20);
            this.tbColumnFamily.TabIndex = 6;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(5, 7);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(64, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "Connection:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(5, 37);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(56, 13);
            this.label7.TabIndex = 1;
            this.label7.Text = "Database:";
            // 
            // tbKeyspace
            // 
            this.tbKeyspace.Location = new System.Drawing.Point(76, 34);
            this.tbKeyspace.Name = "tbKeyspace";
            this.tbKeyspace.ReadOnly = true;
            this.tbKeyspace.Size = new System.Drawing.Size(230, 20);
            this.tbKeyspace.TabIndex = 5;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(5, 70);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(56, 13);
            this.label8.TabIndex = 3;
            this.label8.Text = "Collection:";
            // 
            // tbCluster
            // 
            this.tbCluster.Location = new System.Drawing.Point(76, 4);
            this.tbCluster.Name = "tbCluster";
            this.tbCluster.ReadOnly = true;
            this.tbCluster.Size = new System.Drawing.Size(230, 20);
            this.tbCluster.TabIndex = 4;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(399, 363);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.panStatus);
            this.Controls.Add(this.lblNoStorage);
            this.Controls.Add(this.btnCloseStorage);
            this.Controls.Add(this.gbCurrentStorage);
            this.Controls.Add(this.btnOpenStorage);
            this.helpProvider1.SetHelpKeyword(this, "Introduction");
            this.helpProvider1.SetHelpNavigator(this, System.Windows.Forms.HelpNavigator.KeywordIndex);
            this.Name = "MainForm";
            this.helpProvider1.SetShowHelp(this, true);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Types Visualization";
            this.gbCurrentStorage.ResumeLayout(false);
            this.mongoPanel.ResumeLayout(false);
            this.mongoPanel.PerformLayout();
            this.indexPanel.ResumeLayout(false);
            this.indexPanel.PerformLayout();
            this.gbCreateImageFile.ResumeLayout(false);
            this.gbCreateImageFile.PerformLayout();
            this.panStatus.ResumeLayout(false);
            this.panStatus.PerformLayout();
            this.cassPanel.ResumeLayout(false);
            this.cassPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.Button btnOpenStorage;
        private System.Windows.Forms.GroupBox gbCurrentStorage;
        private System.Windows.Forms.CheckBox cbSafeWrite;
        private System.Windows.Forms.TextBox tbHeader;
        private System.Windows.Forms.TextBox tbClusterSize;
        private System.Windows.Forms.TextBox tbFilePath;
        private System.Windows.Forms.Label lblHeader;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblClusterSize;
        private System.Windows.Forms.Label lblFilePath;
        private System.Windows.Forms.Label lblNoStorage;
        private System.Windows.Forms.Button btnCloseStorage;
        private System.Windows.Forms.GroupBox gbCreateImageFile;
        private System.Windows.Forms.Button btnCreateGVFile;
        private System.Windows.Forms.RadioButton rbSpecificType;
        private System.Windows.Forms.RadioButton rbRootType;
        private System.Windows.Forms.TextBox tbStatus;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Panel panStatus;
        private System.Windows.Forms.Button btnShow;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.HelpProvider helpProvider1;
        private System.Windows.Forms.Panel mongoPanel;
        private System.Windows.Forms.TextBox tbColl;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbDb;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tbConn;
        private System.Windows.Forms.Panel indexPanel;
        private System.Windows.Forms.Panel cassPanel;
        private System.Windows.Forms.TextBox tbColumnFamily;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox tbKeyspace;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox tbCluster;
    }
}

