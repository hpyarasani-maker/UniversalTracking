namespace Receiving
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
            this.errorList = new System.Windows.Forms.ListBox();
            this.lblCount = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.results = new System.Windows.Forms.TextBox();
            this.lstKWs = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // errorList
            // 
            this.errorList.Location = new System.Drawing.Point(411, 289);
            this.errorList.Name = "errorList";
            this.errorList.Size = new System.Drawing.Size(510, 277);
            this.errorList.TabIndex = 30;
            // 
            // lblCount
            // 
            this.lblCount.AutoSize = true;
            this.lblCount.Location = new System.Drawing.Point(829, 42);
            this.lblCount.Name = "lblCount";
            this.lblCount.Size = new System.Drawing.Size(14, 13);
            this.lblCount.TabIndex = 29;
            this.lblCount.Text = "#";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(419, 42);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(14, 13);
            this.label1.TabIndex = 28;
            this.label1.Text = "#";
            // 
            // results
            // 
            this.results.Location = new System.Drawing.Point(411, 67);
            this.results.Multiline = true;
            this.results.Name = "results";
            this.results.Size = new System.Drawing.Size(510, 133);
            this.results.TabIndex = 27;
            // 
            // lstKWs
            // 
            this.lstKWs.FormattingEnabled = true;
            this.lstKWs.Location = new System.Drawing.Point(28, 42);
            this.lstKWs.Name = "lstKWs";
            this.lstKWs.Size = new System.Drawing.Size(364, 524);
            this.lstKWs.TabIndex = 26;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(948, 609);
            this.Controls.Add(this.errorList);
            this.Controls.Add(this.lblCount);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.results);
            this.Controls.Add(this.lstKWs);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox errorList;
        private System.Windows.Forms.Label lblCount;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox results;
        private System.Windows.Forms.ListBox lstKWs;
    }
}

