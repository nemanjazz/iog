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
            this.btnOpenStorage = new System.Windows.Forms.Button();
            this.gbCurrentStorage = new System.Windows.Forms.GroupBox();
            this.gbCreateGVFile = new System.Windows.Forms.GroupBox();
            this.btnCreateGVFile = new System.Windows.Forms.Button();
            this.rbSpecificType = new System.Windows.Forms.RadioButton();
            this.rbRootType = new System.Windows.Forms.RadioButton();
            this.cbSafeWrite = new System.Windows.Forms.CheckBox();
            this.tbHeader = new System.Windows.Forms.TextBox();
            this.tbClusterSize = new System.Windows.Forms.TextBox();
            this.tbFilePath = new System.Windows.Forms.TextBox();
            this.lblHeader = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lblClusterSize = new System.Windows.Forms.Label();
            this.lblFilePath = new System.Windows.Forms.Label();
            this.lblNoStorage = new System.Windows.Forms.Label();
            this.btnCloseStorage = new System.Windows.Forms.Button();
            this.tbStatus = new System.Windows.Forms.TextBox();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.lblStatus = new System.Windows.Forms.Label();
            this.panStatus = new System.Windows.Forms.Panel();
            this.gbCurrentStorage.SuspendLayout();
            this.gbCreateGVFile.SuspendLayout();
            this.panStatus.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOpenStorage
            // 
            this.btnOpenStorage.Location = new System.Drawing.Point(34, 23);
            this.btnOpenStorage.Name = "btnOpenStorage";
            this.btnOpenStorage.Size = new System.Drawing.Size(72, 36);
            this.btnOpenStorage.TabIndex = 0;
            this.btnOpenStorage.Text = "Open Storage File";
            this.btnOpenStorage.UseVisualStyleBackColor = true;
            this.btnOpenStorage.Click += new System.EventHandler(this.btnOpenStorage_Click);
            // 
            // gbCurrentStorage
            // 
            this.gbCurrentStorage.Controls.Add(this.gbCreateGVFile);
            this.gbCurrentStorage.Controls.Add(this.cbSafeWrite);
            this.gbCurrentStorage.Controls.Add(this.tbHeader);
            this.gbCurrentStorage.Controls.Add(this.tbClusterSize);
            this.gbCurrentStorage.Controls.Add(this.tbFilePath);
            this.gbCurrentStorage.Controls.Add(this.lblHeader);
            this.gbCurrentStorage.Controls.Add(this.label3);
            this.gbCurrentStorage.Controls.Add(this.lblClusterSize);
            this.gbCurrentStorage.Controls.Add(this.lblFilePath);
            this.gbCurrentStorage.Location = new System.Drawing.Point(27, 75);
            this.gbCurrentStorage.Name = "gbCurrentStorage";
            this.gbCurrentStorage.Size = new System.Drawing.Size(343, 200);
            this.gbCurrentStorage.TabIndex = 1;
            this.gbCurrentStorage.TabStop = false;
            this.gbCurrentStorage.Text = "Current storage opened";
            // 
            // gbCreateGVFile
            // 
            this.gbCreateGVFile.Controls.Add(this.btnCreateGVFile);
            this.gbCreateGVFile.Controls.Add(this.rbSpecificType);
            this.gbCreateGVFile.Controls.Add(this.rbRootType);
            this.gbCreateGVFile.Location = new System.Drawing.Point(7, 120);
            this.gbCreateGVFile.Name = "gbCreateGVFile";
            this.gbCreateGVFile.Size = new System.Drawing.Size(330, 67);
            this.gbCreateGVFile.TabIndex = 12;
            this.gbCreateGVFile.TabStop = false;
            this.gbCreateGVFile.Text = "Create GraphViz File";
            // 
            // btnCreateGVFile
            // 
            this.btnCreateGVFile.Location = new System.Drawing.Point(234, 27);
            this.btnCreateGVFile.Name = "btnCreateGVFile";
            this.btnCreateGVFile.Size = new System.Drawing.Size(75, 23);
            this.btnCreateGVFile.TabIndex = 11;
            this.btnCreateGVFile.Text = "Ok";
            this.btnCreateGVFile.UseVisualStyleBackColor = true;
            this.btnCreateGVFile.Click += new System.EventHandler(this.btnCreateGVFile_Click);
            // 
            // rbSpecificType
            // 
            this.rbSpecificType.AutoSize = true;
            this.rbSpecificType.Location = new System.Drawing.Point(115, 30);
            this.rbSpecificType.Name = "rbSpecificType";
            this.rbSpecificType.Size = new System.Drawing.Size(113, 17);
            this.rbSpecificType.TabIndex = 10;
            this.rbSpecificType.Text = "from Specific Type";
            this.rbSpecificType.UseVisualStyleBackColor = true;
            // 
            // rbRootType
            // 
            this.rbRootType.AutoSize = true;
            this.rbRootType.Checked = true;
            this.rbRootType.Location = new System.Drawing.Point(11, 30);
            this.rbRootType.Name = "rbRootType";
            this.rbRootType.Size = new System.Drawing.Size(98, 17);
            this.rbRootType.TabIndex = 9;
            this.rbRootType.TabStop = true;
            this.rbRootType.Text = "from Root Type";
            this.rbRootType.UseVisualStyleBackColor = true;
            // 
            // cbSafeWrite
            // 
            this.cbSafeWrite.AutoSize = true;
            this.cbSafeWrite.Enabled = false;
            this.cbSafeWrite.Location = new System.Drawing.Point(86, 100);
            this.cbSafeWrite.Name = "cbSafeWrite";
            this.cbSafeWrite.Size = new System.Drawing.Size(15, 14);
            this.cbSafeWrite.TabIndex = 7;
            this.cbSafeWrite.UseVisualStyleBackColor = true;
            // 
            // tbHeader
            // 
            this.tbHeader.Location = new System.Drawing.Point(86, 73);
            this.tbHeader.Name = "tbHeader";
            this.tbHeader.ReadOnly = true;
            this.tbHeader.Size = new System.Drawing.Size(78, 20);
            this.tbHeader.TabIndex = 6;
            // 
            // tbClusterSize
            // 
            this.tbClusterSize.Location = new System.Drawing.Point(86, 48);
            this.tbClusterSize.Name = "tbClusterSize";
            this.tbClusterSize.ReadOnly = true;
            this.tbClusterSize.Size = new System.Drawing.Size(46, 20);
            this.tbClusterSize.TabIndex = 5;
            // 
            // tbFilePath
            // 
            this.tbFilePath.Location = new System.Drawing.Point(86, 22);
            this.tbFilePath.Name = "tbFilePath";
            this.tbFilePath.ReadOnly = true;
            this.tbFilePath.Size = new System.Drawing.Size(230, 20);
            this.tbFilePath.TabIndex = 4;
            // 
            // lblHeader
            // 
            this.lblHeader.AutoSize = true;
            this.lblHeader.Location = new System.Drawing.Point(15, 76);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Size = new System.Drawing.Size(45, 13);
            this.lblHeader.TabIndex = 3;
            this.lblHeader.Text = "Header:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(15, 101);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(60, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Safe Write:";
            // 
            // lblClusterSize
            // 
            this.lblClusterSize.AutoSize = true;
            this.lblClusterSize.Location = new System.Drawing.Point(15, 51);
            this.lblClusterSize.Name = "lblClusterSize";
            this.lblClusterSize.Size = new System.Drawing.Size(65, 13);
            this.lblClusterSize.TabIndex = 1;
            this.lblClusterSize.Text = "Cluster Size:";
            // 
            // lblFilePath
            // 
            this.lblFilePath.AutoSize = true;
            this.lblFilePath.Location = new System.Drawing.Point(15, 25);
            this.lblFilePath.Name = "lblFilePath";
            this.lblFilePath.Size = new System.Drawing.Size(50, 13);
            this.lblFilePath.TabIndex = 0;
            this.lblFilePath.Text = "File path:";
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
            this.btnCloseStorage.TabIndex = 2;
            this.btnCloseStorage.Text = "Close Storage File";
            this.btnCloseStorage.UseVisualStyleBackColor = true;
            this.btnCloseStorage.Click += new System.EventHandler(this.btnCloseStorage_Click);
            // 
            // tbStatus
            // 
            this.tbStatus.Location = new System.Drawing.Point(7, 19);
            this.tbStatus.Name = "tbStatus";
            this.tbStatus.ReadOnly = true;
            this.tbStatus.Size = new System.Drawing.Size(336, 20);
            this.tbStatus.TabIndex = 3;
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(4, 3);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(158, 13);
            this.lblStatus.TabIndex = 4;
            this.lblStatus.Text = "GraphViz file has been created: ";
            // 
            // panStatus
            // 
            this.panStatus.Controls.Add(this.lblStatus);
            this.panStatus.Controls.Add(this.tbStatus);
            this.panStatus.Location = new System.Drawing.Point(27, 281);
            this.panStatus.Name = "panStatus";
            this.panStatus.Size = new System.Drawing.Size(360, 44);
            this.panStatus.TabIndex = 5;
            this.panStatus.Visible = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(399, 332);
            this.Controls.Add(this.panStatus);
            this.Controls.Add(this.lblNoStorage);
            this.Controls.Add(this.btnCloseStorage);
            this.Controls.Add(this.gbCurrentStorage);
            this.Controls.Add(this.btnOpenStorage);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Types Visualisation";
            this.gbCurrentStorage.ResumeLayout(false);
            this.gbCurrentStorage.PerformLayout();
            this.gbCreateGVFile.ResumeLayout(false);
            this.gbCreateGVFile.PerformLayout();
            this.panStatus.ResumeLayout(false);
            this.panStatus.PerformLayout();
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
        private System.Windows.Forms.GroupBox gbCreateGVFile;
        private System.Windows.Forms.Button btnCreateGVFile;
        private System.Windows.Forms.RadioButton rbSpecificType;
        private System.Windows.Forms.RadioButton rbRootType;
        private System.Windows.Forms.TextBox tbStatus;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Panel panStatus;
    }
}

