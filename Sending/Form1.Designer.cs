namespace Sending
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
            this.worklist = new System.Windows.Forms.ListBox();
            this.results = new System.Windows.Forms.TextBox();
            this.lblIP = new System.Windows.Forms.Label();
            this.progress_lbl = new System.Windows.Forms.Label();
            this.date_picker = new System.Windows.Forms.DateTimePicker();
            this.SuspendLayout();
            // 
            // errorList
            // 
            this.errorList.FormattingEnabled = true;
            this.errorList.Location = new System.Drawing.Point(263, 219);
            this.errorList.Name = "errorList";
            this.errorList.Size = new System.Drawing.Size(736, 394);
            this.errorList.TabIndex = 27;
            // 
            // lblCount
            // 
            this.lblCount.AutoSize = true;
            this.lblCount.Location = new System.Drawing.Point(266, 175);
            this.lblCount.Name = "lblCount";
            this.lblCount.Size = new System.Drawing.Size(14, 13);
            this.lblCount.TabIndex = 26;
            this.lblCount.Text = "#";
            // 
            // worklist
            // 
            this.worklist.FormattingEnabled = true;
            this.worklist.Location = new System.Drawing.Point(13, 64);
            this.worklist.Name = "worklist";
            this.worklist.Size = new System.Drawing.Size(244, 550);
            this.worklist.TabIndex = 25;
            // 
            // results
            // 
            this.results.Location = new System.Drawing.Point(263, 63);
            this.results.Multiline = true;
            this.results.Name = "results";
            this.results.ScrollBars = System.Windows.Forms.ScrollBars.Horizontal;
            this.results.Size = new System.Drawing.Size(736, 96);
            this.results.TabIndex = 24;
            // 
            // lblIP
            // 
            this.lblIP.AutoSize = true;
            this.lblIP.Location = new System.Drawing.Point(621, 37);
            this.lblIP.Name = "lblIP";
            this.lblIP.Size = new System.Drawing.Size(31, 13);
            this.lblIP.TabIndex = 23;
            this.lblIP.Text = "ip_lbl";
            // 
            // progress_lbl
            // 
            this.progress_lbl.Location = new System.Drawing.Point(263, 28);
            this.progress_lbl.Name = "progress_lbl";
            this.progress_lbl.Size = new System.Drawing.Size(272, 23);
            this.progress_lbl.TabIndex = 22;
            // 
            // date_picker
            // 
            this.date_picker.CustomFormat = "dd/mm/yyyy";
            this.date_picker.Location = new System.Drawing.Point(13, 31);
            this.date_picker.Name = "date_picker";
            this.date_picker.Size = new System.Drawing.Size(228, 20);
            this.date_picker.TabIndex = 21;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1020, 647);
            this.Controls.Add(this.errorList);
            this.Controls.Add(this.lblCount);
            this.Controls.Add(this.worklist);
            this.Controls.Add(this.results);
            this.Controls.Add(this.lblIP);
            this.Controls.Add(this.progress_lbl);
            this.Controls.Add(this.date_picker);
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox errorList;
        private System.Windows.Forms.Label lblCount;
        private System.Windows.Forms.ListBox worklist;
        private System.Windows.Forms.TextBox results;
        private System.Windows.Forms.Label lblIP;
        private System.Windows.Forms.Label progress_lbl;
        private System.Windows.Forms.DateTimePicker date_picker;
    }
}

