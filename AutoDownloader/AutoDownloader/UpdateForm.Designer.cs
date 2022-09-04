namespace AutoDownloader
{
    partial class UpdateForm
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
            this.ChangeLog = new System.Windows.Forms.RichTextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.DownloadUpdate = new System.Windows.Forms.Button();
            this.DontShow = new System.Windows.Forms.CheckBox();
            this.Continue = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ChangeLog
            // 
            this.ChangeLog.Location = new System.Drawing.Point(12, 28);
            this.ChangeLog.Name = "ChangeLog";
            this.ChangeLog.ReadOnly = true;
            this.ChangeLog.Size = new System.Drawing.Size(433, 227);
            this.ChangeLog.TabIndex = 24;
            this.ChangeLog.Text = "";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 12);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(68, 13);
            this.label2.TabIndex = 25;
            this.label2.Text = "Change Log:";
            // 
            // DownloadUpdate
            // 
            this.DownloadUpdate.Location = new System.Drawing.Point(12, 261);
            this.DownloadUpdate.Name = "DownloadUpdate";
            this.DownloadUpdate.Size = new System.Drawing.Size(240, 23);
            this.DownloadUpdate.TabIndex = 26;
            this.DownloadUpdate.Text = "Update now";
            this.DownloadUpdate.UseVisualStyleBackColor = true;
            this.DownloadUpdate.Click += new System.EventHandler(this.DownloadUpdate_Click);
            // 
            // DontShow
            // 
            this.DontShow.AutoSize = true;
            this.DontShow.Location = new System.Drawing.Point(339, 265);
            this.DontShow.Name = "DontShow";
            this.DontShow.Size = new System.Drawing.Size(106, 17);
            this.DontShow.TabIndex = 27;
            this.DontShow.Text = "Dont show again";
            this.DontShow.UseVisualStyleBackColor = true;
            // 
            // Continue
            // 
            this.Continue.Location = new System.Drawing.Point(258, 261);
            this.Continue.Name = "Continue";
            this.Continue.Size = new System.Drawing.Size(75, 23);
            this.Continue.TabIndex = 28;
            this.Continue.Text = "Continue";
            this.Continue.UseVisualStyleBackColor = true;
            this.Continue.Click += new System.EventHandler(this.Continue_Click);
            // 
            // UpdateForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(457, 296);
            this.Controls.Add(this.Continue);
            this.Controls.Add(this.DontShow);
            this.Controls.Add(this.DownloadUpdate);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.ChangeLog);
            this.MaximumSize = new System.Drawing.Size(473, 335);
            this.MinimumSize = new System.Drawing.Size(473, 335);
            this.Name = "UpdateForm";
            this.Text = "A new update is available!";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.UpdateForm_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox ChangeLog;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button DownloadUpdate;
        private System.Windows.Forms.CheckBox DontShow;
        private System.Windows.Forms.Button Continue;
    }
}