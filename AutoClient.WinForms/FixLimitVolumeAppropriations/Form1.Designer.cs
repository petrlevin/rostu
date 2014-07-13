namespace AutoClient.FixLimitVolumeAppropriations
{
    partial class Form1
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
            this.btnLogin = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.btnLeafStatuses = new System.Windows.Forms.Button();
            this.btnDeleteDrafts = new System.Windows.Forms.Button();
            this.btnInfoDrafts = new System.Windows.Forms.Button();
            this.btnInfoDraftsInEditMode = new System.Windows.Forms.Button();
            this.btnFinishEditing = new System.Windows.Forms.Button();
            this.btnUndoChange = new System.Windows.Forms.Button();
            this.btnLeafsInfo = new System.Windows.Forms.Button();
            this.btnChange = new System.Windows.Forms.Button();
            this.btnProcess = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnLogin
            // 
            this.btnLogin.Location = new System.Drawing.Point(13, 13);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(75, 23);
            this.btnLogin.TabIndex = 0;
            this.btnLogin.Text = "Login";
            this.btnLogin.UseVisualStyleBackColor = true;
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.Location = new System.Drawing.Point(13, 102);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox1.Size = new System.Drawing.Size(763, 488);
            this.textBox1.TabIndex = 2;
            // 
            // btnLeafStatuses
            // 
            this.btnLeafStatuses.Location = new System.Drawing.Point(94, 12);
            this.btnLeafStatuses.Name = "btnLeafStatuses";
            this.btnLeafStatuses.Size = new System.Drawing.Size(93, 23);
            this.btnLeafStatuses.TabIndex = 3;
            this.btnLeafStatuses.Text = "LeafStatuses";
            this.btnLeafStatuses.UseVisualStyleBackColor = true;
            this.btnLeafStatuses.Click += new System.EventHandler(this.btnLeafStatuses_Click);
            // 
            // btnDeleteDrafts
            // 
            this.btnDeleteDrafts.Location = new System.Drawing.Point(357, 42);
            this.btnDeleteDrafts.Name = "btnDeleteDrafts";
            this.btnDeleteDrafts.Size = new System.Drawing.Size(136, 23);
            this.btnDeleteDrafts.TabIndex = 4;
            this.btnDeleteDrafts.Text = "DeleteDrafts";
            this.btnDeleteDrafts.UseVisualStyleBackColor = true;
            this.btnDeleteDrafts.Click += new System.EventHandler(this.btnDeleteDrafts_Click);
            // 
            // btnInfoDrafts
            // 
            this.btnInfoDrafts.Location = new System.Drawing.Point(357, 12);
            this.btnInfoDrafts.Name = "btnInfoDrafts";
            this.btnInfoDrafts.Size = new System.Drawing.Size(136, 23);
            this.btnInfoDrafts.TabIndex = 5;
            this.btnInfoDrafts.Text = "InfoDrafts";
            this.btnInfoDrafts.UseVisualStyleBackColor = true;
            this.btnInfoDrafts.Click += new System.EventHandler(this.btnInfoDrafts_Click);
            // 
            // btnInfoDraftsInEditMode
            // 
            this.btnInfoDraftsInEditMode.Location = new System.Drawing.Point(193, 12);
            this.btnInfoDraftsInEditMode.Name = "btnInfoDraftsInEditMode";
            this.btnInfoDraftsInEditMode.Size = new System.Drawing.Size(158, 23);
            this.btnInfoDraftsInEditMode.TabIndex = 6;
            this.btnInfoDraftsInEditMode.Text = "InfoDraftsInEditMode";
            this.btnInfoDraftsInEditMode.UseVisualStyleBackColor = true;
            this.btnInfoDraftsInEditMode.Click += new System.EventHandler(this.btnInfoDraftsInEditMode_Click);
            // 
            // btnFinishEditing
            // 
            this.btnFinishEditing.Location = new System.Drawing.Point(193, 43);
            this.btnFinishEditing.Name = "btnFinishEditing";
            this.btnFinishEditing.Size = new System.Drawing.Size(158, 23);
            this.btnFinishEditing.TabIndex = 7;
            this.btnFinishEditing.Text = "Отменить редактирование";
            this.btnFinishEditing.UseVisualStyleBackColor = true;
            this.btnFinishEditing.Click += new System.EventHandler(this.btnFinishEditing_Click);
            // 
            // btnUndoChange
            // 
            this.btnUndoChange.Location = new System.Drawing.Point(357, 72);
            this.btnUndoChange.Name = "btnUndoChange";
            this.btnUndoChange.Size = new System.Drawing.Size(136, 23);
            this.btnUndoChange.TabIndex = 8;
            this.btnUndoChange.Text = "UndoChange";
            this.btnUndoChange.UseVisualStyleBackColor = true;
            this.btnUndoChange.Click += new System.EventHandler(this.btnUndoChange_Click);
            // 
            // btnLeafsInfo
            // 
            this.btnLeafsInfo.Location = new System.Drawing.Point(500, 13);
            this.btnLeafsInfo.Name = "btnLeafsInfo";
            this.btnLeafsInfo.Size = new System.Drawing.Size(96, 23);
            this.btnLeafsInfo.TabIndex = 9;
            this.btnLeafsInfo.Text = "LeafsInfo";
            this.btnLeafsInfo.UseVisualStyleBackColor = true;
            this.btnLeafsInfo.Click += new System.EventHandler(this.btnLeafsInfo_Click);
            // 
            // btnChange
            // 
            this.btnChange.Location = new System.Drawing.Point(500, 43);
            this.btnChange.Name = "btnChange";
            this.btnChange.Size = new System.Drawing.Size(96, 23);
            this.btnChange.TabIndex = 10;
            this.btnChange.Text = "Change";
            this.btnChange.UseVisualStyleBackColor = true;
            this.btnChange.Click += new System.EventHandler(this.btnChange_Click);
            // 
            // btnProcess
            // 
            this.btnProcess.Location = new System.Drawing.Point(500, 72);
            this.btnProcess.Name = "btnProcess";
            this.btnProcess.Size = new System.Drawing.Size(96, 23);
            this.btnProcess.TabIndex = 11;
            this.btnProcess.Text = "Process";
            this.btnProcess.UseVisualStyleBackColor = true;
            this.btnProcess.Click += new System.EventHandler(this.btnProcess_Click);
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(13, 42);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(75, 43);
            this.btnClear.TabIndex = 12;
            this.btnClear.Text = "Очистить окно";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(788, 602);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.btnProcess);
            this.Controls.Add(this.btnChange);
            this.Controls.Add(this.btnLeafsInfo);
            this.Controls.Add(this.btnUndoChange);
            this.Controls.Add(this.btnFinishEditing);
            this.Controls.Add(this.btnInfoDraftsInEditMode);
            this.Controls.Add(this.btnInfoDrafts);
            this.Controls.Add(this.btnDeleteDrafts);
            this.Controls.Add(this.btnLeafStatuses);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.btnLogin);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnLogin;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button btnLeafStatuses;
        private System.Windows.Forms.Button btnDeleteDrafts;
        private System.Windows.Forms.Button btnInfoDrafts;
        private System.Windows.Forms.Button btnInfoDraftsInEditMode;
        private System.Windows.Forms.Button btnFinishEditing;
        private System.Windows.Forms.Button btnUndoChange;
        private System.Windows.Forms.Button btnLeafsInfo;
        private System.Windows.Forms.Button btnChange;
        private System.Windows.Forms.Button btnProcess;
        private System.Windows.Forms.Button btnClear;
    }
}