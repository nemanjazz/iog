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
            this.SuspendLayout();
            // 
            // lblFilePath
            // 
            this.lblFilePath.AutoSize = true;
            this.lblFilePath.Location = new System.Drawing.Point(22, 25);
            this.lblFilePath.Name = "lblFilePath";
            this.lblFilePath.Size = new System.Drawing.Size(60, 13);
            this.lblFilePath.TabIndex = 0;
            this.lblFilePath.Text = "File path (*)";
            // 
            // lblClusterSize
            // 
            this.lblClusterSize.AutoSize = true;
            this.lblClusterSize.Location = new System.Drawing.Point(22, 80);
            this.lblClusterSize.Name = "lblClusterSize";
            this.lblClusterSize.Size = new System.Drawing.Size(75, 13);
            this.lblClusterSize.TabIndex = 1;
            this.lblClusterSize.Text = "Cluster Size (*)";
            // 
            // lblSafeWrite
            // 
            this.lblSafeWrite.AutoSize = true;
            this.lblSafeWrite.Location = new System.Drawing.Point(22, 130);
            this.lblSafeWrite.Name = "lblSafeWrite";
            this.lblSafeWrite.Size = new System.Drawing.Size(57, 13);
            this.lblSafeWrite.TabIndex = 2;
            this.lblSafeWrite.Text = "Safe Write";
            // 
            // lblHeader
            // 
            this.lblHeader.AutoSize = true;
            this.lblHeader.Location = new System.Drawing.Point(22, 106);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Size = new System.Drawing.Size(42, 13);
            this.lblHeader.TabIndex = 3;
            this.lblHeader.Text = "Header";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // tbFilePath
            // 
            this.tbFilePath.Location = new System.Drawing.Point(129, 22);
            this.tbFilePath.Name = "tbFilePath";
            this.tbFilePath.Size = new System.Drawing.Size(233, 20);
            this.tbFilePath.TabIndex = 4;
            // 
            // tbClusterSize
            // 
            this.tbClusterSize.Location = new System.Drawing.Point(129, 77);
            this.tbClusterSize.Name = "tbClusterSize";
            this.tbClusterSize.Size = new System.Drawing.Size(44, 20);
            this.tbClusterSize.TabIndex = 5;
            // 
            // tbHeader
            // 
            this.tbHeader.Location = new System.Drawing.Point(129, 103);
            this.tbHeader.Name = "tbHeader";
            this.tbHeader.Size = new System.Drawing.Size(67, 20);
            this.tbHeader.TabIndex = 6;
            // 
            // checkSafeWrite
            // 
            this.checkSafeWrite.AutoSize = true;
            this.checkSafeWrite.Checked = true;
            this.checkSafeWrite.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkSafeWrite.Location = new System.Drawing.Point(129, 130);
            this.checkSafeWrite.Name = "checkSafeWrite";
            this.checkSafeWrite.Size = new System.Drawing.Size(15, 14);
            this.checkSafeWrite.TabIndex = 7;
            this.checkSafeWrite.UseVisualStyleBackColor = true;
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(129, 48);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnBrowse.TabIndex = 8;
            this.btnBrowse.Text = "Browse";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStatus.ForeColor = System.Drawing.Color.Black;
            this.lblStatus.Location = new System.Drawing.Point(22, 152);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(47, 13);
            this.lblStatus.TabIndex = 9;
            this.lblStatus.Text = "lblStatus";
            // 
            // btnOk
            // 
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Location = new System.Drawing.Point(206, 180);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 11;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(287, 180);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 12;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // OpenStorage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(385, 215);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.checkSafeWrite);
            this.Controls.Add(this.tbHeader);
            this.Controls.Add(this.tbClusterSize);
            this.Controls.Add(this.tbFilePath);
            this.Controls.Add(this.lblHeader);
            this.Controls.Add(this.lblSafeWrite);
            this.Controls.Add(this.lblClusterSize);
            this.Controls.Add(this.lblFilePath);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "OpenStorage";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Open Storage";
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
    }
}