namespace UniversalTracking
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
            this.lblCount = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.results = new System.Windows.Forms.TextBox();
            this.lstKWs = new System.Windows.Forms.ListBox();
            this.errorList = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // lblCount
            // 
            this.lblCount.AutoSize = true;
            this.lblCount.Location = new System.Drawing.Point(759, 48);
            this.lblCount.Name = "lblCount";
            this.lblCount.Size = new System.Drawing.Size(14, 13);
            this.lblCount.TabIndex = 14;
            this.lblCount.Text = "#";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(346, 48);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(14, 13);
            this.label1.TabIndex = 13;
            this.label1.Text = "#";
            // 
            // results
            // 
            this.results.Location = new System.Drawing.Point(349, 73);
            this.results.Multiline = true;
            this.results.Name = "results";
            this.results.Size = new System.Drawing.Size(510, 108);
            this.results.TabIndex = 11;
            // 
            // lstKWs
            // 
            this.lstKWs.FormattingEnabled = true;
            this.lstKWs.Location = new System.Drawing.Point(24, 37);
            this.lstKWs.Name = "lstKWs";
            this.lstKWs.Size = new System.Drawing.Size(310, 472);
            this.lstKWs.TabIndex = 10;
            // 
            // errorList
            // 
            this.errorList.Location = new System.Drawing.Point(349, 261);
            this.errorList.Name = "errorList";
            this.errorList.Size = new System.Drawing.Size(510, 238);
            this.errorList.TabIndex = 15;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(885, 535);
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

        private System.Windows.Forms.Label lblCount;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox results;
        private System.Windows.Forms.ListBox lstKWs;
        private System.Windows.Forms.ListBox errorList;
    }
}

