namespace Execom.IOG.TypesVisualisationApp
{
    partial class ChooseTypeForm
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
            this.listBoxParents = new System.Windows.Forms.ListBox();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblParents = new System.Windows.Forms.Label();
            this.listBoxChildren = new System.Windows.Forms.ListBox();
            this.tbCurrentType = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.lblCurrentType = new System.Windows.Forms.Label();
            this.btnBackToRoot = new System.Windows.Forms.Button();
            this.btnSearchType = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.helpProvider1 = new System.Windows.Forms.HelpProvider();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // listBoxParents
            // 
            this.listBoxParents.BackColor = System.Drawing.SystemColors.InactiveBorder;
            this.listBoxParents.FormattingEnabled = true;
            this.listBoxParents.Location = new System.Drawing.Point(12, 28);
            this.listBoxParents.Name = "listBoxParents";
            this.listBoxParents.Size = new System.Drawing.Size(161, 199);
            this.listBoxParents.TabIndex = 0;
            this.toolTip1.SetToolTip(this.listBoxParents, "List of the parent types \r\nfor the current type");
            this.listBoxParents.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listboxTypes_KeyDown);
            this.listBoxParents.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listboxTypes_MouseDoubleClick);
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(356, 239);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(80, 23);
            this.btnOk.TabIndex = 2;
            this.btnOk.Text = "Create Image";
            this.toolTip1.SetToolTip(this.btnOk, "Create image");
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(442, 239);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.toolTip1.SetToolTip(this.btnCancel, "Cancel");
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // lblParents
            // 
            this.lblParents.AutoSize = true;
            this.lblParents.Location = new System.Drawing.Point(68, 9);
            this.lblParents.Name = "lblParents";
            this.lblParents.Size = new System.Drawing.Size(43, 13);
            this.lblParents.TabIndex = 3;
            this.lblParents.Text = "Parents";
            // 
            // listBoxChildren
            // 
            this.listBoxChildren.BackColor = System.Drawing.SystemColors.InactiveBorder;
            this.listBoxChildren.FormattingEnabled = true;
            this.listBoxChildren.Location = new System.Drawing.Point(356, 28);
            this.listBoxChildren.Name = "listBoxChildren";
            this.listBoxChildren.Size = new System.Drawing.Size(161, 199);
            this.listBoxChildren.TabIndex = 4;
            this.toolTip1.SetToolTip(this.listBoxChildren, "List of the children types\r\nfor the current type");
            this.listBoxChildren.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listboxTypes_KeyDown);
            this.listBoxChildren.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listboxTypes_MouseDoubleClick);
            // 
            // tbCurrentType
            // 
            this.tbCurrentType.BackColor = System.Drawing.SystemColors.InactiveBorder;
            this.tbCurrentType.Location = new System.Drawing.Point(179, 116);
            this.tbCurrentType.Name = "tbCurrentType";
            this.tbCurrentType.ReadOnly = true;
            this.tbCurrentType.Size = new System.Drawing.Size(171, 20);
            this.tbCurrentType.TabIndex = 5;
            this.tbCurrentType.Text = "Current type";
            this.tbCurrentType.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(414, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(45, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Children";
            // 
            // lblCurrentType
            // 
            this.lblCurrentType.AutoSize = true;
            this.lblCurrentType.Location = new System.Drawing.Point(233, 97);
            this.lblCurrentType.Name = "lblCurrentType";
            this.lblCurrentType.Size = new System.Drawing.Size(64, 13);
            this.lblCurrentType.TabIndex = 7;
            this.lblCurrentType.Text = "Current type";
            // 
            // btnBackToRoot
            // 
            this.btnBackToRoot.Location = new System.Drawing.Point(226, 143);
            this.btnBackToRoot.Name = "btnBackToRoot";
            this.btnBackToRoot.Size = new System.Drawing.Size(78, 23);
            this.btnBackToRoot.TabIndex = 0;
            this.btnBackToRoot.Text = "Back to root";
            this.toolTip1.SetToolTip(this.btnBackToRoot, "Set root type as \r\nthe current type");
            this.btnBackToRoot.UseVisualStyleBackColor = true;
            this.btnBackToRoot.Click += new System.EventHandler(this.btnBackToRoot_Click);
            // 
            // btnSearchType
            // 
            this.btnSearchType.Location = new System.Drawing.Point(226, 172);
            this.btnSearchType.Name = "btnSearchType";
            this.btnSearchType.Size = new System.Drawing.Size(78, 23);
            this.btnSearchType.TabIndex = 1;
            this.btnSearchType.Text = "Search type";
            this.toolTip1.SetToolTip(this.btnSearchType, "Search for a specific type");
            this.btnSearchType.UseVisualStyleBackColor = true;
            this.btnSearchType.Click += new System.EventHandler(this.btnSearchType_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 257);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(90, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Press \'F1\' for help";
            // 
            // helpProvider1
            // 
            this.helpProvider1.HelpNamespace = "..\\..\\help\\help.chm";
            // 
            // ChooseTypeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(529, 279);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnSearchType);
            this.Controls.Add(this.btnBackToRoot);
            this.Controls.Add(this.lblCurrentType);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tbCurrentType);
            this.Controls.Add(this.listBoxChildren);
            this.Controls.Add(this.lblParents);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.listBoxParents);
            this.helpProvider1.SetHelpKeyword(this, "Opening storage file");
            this.helpProvider1.SetHelpNavigator(this, System.Windows.Forms.HelpNavigator.KeywordIndex);
            this.Name = "ChooseTypeForm";
            this.helpProvider1.SetShowHelp(this, true);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Choose Type";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox listBoxParents;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblParents;
        private System.Windows.Forms.ListBox listBoxChildren;
        private System.Windows.Forms.TextBox tbCurrentType;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblCurrentType;
        private System.Windows.Forms.Button btnBackToRoot;
        private System.Windows.Forms.Button btnSearchType;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.HelpProvider helpProvider1;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}