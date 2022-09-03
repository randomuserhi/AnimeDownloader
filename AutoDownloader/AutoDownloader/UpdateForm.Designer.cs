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
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.DownloadUpdate = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(12, 28);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.Size = new System.Drawing.Size(433, 227);
            this.richTextBox1.TabIndex = 24;
            this.richTextBox1.Text = "";
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
            this.DownloadUpdate.Size = new System.Drawing.Size(433, 23);
            this.DownloadUpdate.TabIndex = 26;
            this.DownloadUpdate.Text = "Update now";
            this.DownloadUpdate.UseVisualStyleBackColor = true;
            // 
            // UpdateForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(457, 296);
            this.Controls.Add(this.DownloadUpdate);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.richTextBox1);
            this.MaximumSize = new System.Drawing.Size(473, 335);
            this.MinimumSize = new System.Drawing.Size(473, 335);
            this.Name = "UpdateForm";
            this.Text = "A new update is available!";
            this.Controls.SetChildIndex(this.Downloads, 0);
            this.Controls.SetChildIndex(this.Cancel, 0);
            this.Controls.SetChildIndex(this.SavePathLabel, 0);
            this.Controls.SetChildIndex(this.DubbedButton, 0);
            this.Controls.SetChildIndex(this.SubbedButton, 0);
            this.Controls.SetChildIndex(this.richTextBox1, 0);
            this.Controls.SetChildIndex(this.label2, 0);
            this.Controls.SetChildIndex(this.DownloadUpdate, 0);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button DownloadUpdate;
    }
}