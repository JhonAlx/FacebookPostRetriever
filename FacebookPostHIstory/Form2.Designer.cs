namespace FacebookPostHIstory
{
    partial class Form2
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.DownloadButton = new System.Windows.Forms.Button();
            this.UntilDateTimePicker = new System.Windows.Forms.DateTimePicker();
            this.SinceDateTimePicker = new System.Windows.Forms.DateTimePicker();
            this.UntilCheckBox = new System.Windows.Forms.CheckBox();
            this.SinceCheckBox = new System.Windows.Forms.CheckBox();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.AccessTokenStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusStrip = new System.Windows.Forms.StatusStrip();
            this.groupBox1.SuspendLayout();
            this.StatusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.DownloadButton);
            this.groupBox1.Controls.Add(this.UntilDateTimePicker);
            this.groupBox1.Controls.Add(this.SinceDateTimePicker);
            this.groupBox1.Controls.Add(this.UntilCheckBox);
            this.groupBox1.Controls.Add(this.SinceCheckBox);
            this.groupBox1.Location = new System.Drawing.Point(13, 13);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(840, 52);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            // 
            // DownloadButton
            // 
            this.DownloadButton.Location = new System.Drawing.Point(531, 16);
            this.DownloadButton.Name = "DownloadButton";
            this.DownloadButton.Size = new System.Drawing.Size(100, 23);
            this.DownloadButton.TabIndex = 4;
            this.DownloadButton.Text = "Download data";
            this.DownloadButton.UseVisualStyleBackColor = true;
            this.DownloadButton.Click += new System.EventHandler(this.DownloadButton_Click);
            // 
            // UntilDateTimePicker
            // 
            this.UntilDateTimePicker.Enabled = false;
            this.UntilDateTimePicker.Location = new System.Drawing.Point(325, 17);
            this.UntilDateTimePicker.Name = "UntilDateTimePicker";
            this.UntilDateTimePicker.Size = new System.Drawing.Size(200, 20);
            this.UntilDateTimePicker.TabIndex = 3;
            // 
            // SinceDateTimePicker
            // 
            this.SinceDateTimePicker.Enabled = false;
            this.SinceDateTimePicker.Location = new System.Drawing.Point(66, 17);
            this.SinceDateTimePicker.Name = "SinceDateTimePicker";
            this.SinceDateTimePicker.Size = new System.Drawing.Size(200, 20);
            this.SinceDateTimePicker.TabIndex = 1;
            // 
            // UntilCheckBox
            // 
            this.UntilCheckBox.AutoSize = true;
            this.UntilCheckBox.Location = new System.Drawing.Point(272, 20);
            this.UntilCheckBox.Name = "UntilCheckBox";
            this.UntilCheckBox.Size = new System.Drawing.Size(47, 17);
            this.UntilCheckBox.TabIndex = 2;
            this.UntilCheckBox.Text = "Until";
            this.UntilCheckBox.UseVisualStyleBackColor = true;
            this.UntilCheckBox.CheckedChanged += new System.EventHandler(this.UntilCheckBox_CheckedChanged);
            // 
            // SinceCheckBox
            // 
            this.SinceCheckBox.AutoSize = true;
            this.SinceCheckBox.Location = new System.Drawing.Point(7, 20);
            this.SinceCheckBox.Name = "SinceCheckBox";
            this.SinceCheckBox.Size = new System.Drawing.Size(53, 17);
            this.SinceCheckBox.TabIndex = 0;
            this.SinceCheckBox.Text = "Since";
            this.SinceCheckBox.UseVisualStyleBackColor = true;
            this.SinceCheckBox.CheckedChanged += new System.EventHandler(this.SinceCheckBox_CheckedChanged);
            // 
            // AccessTokenStatusLabel
            // 
            this.AccessTokenStatusLabel.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.AccessTokenStatusLabel.Name = "AccessTokenStatusLabel";
            this.AccessTokenStatusLabel.Size = new System.Drawing.Size(139, 19);
            this.AccessTokenStatusLabel.Text = "AccessTokenStatusLabel";
            // 
            // StatusStrip
            // 
            this.StatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.AccessTokenStatusLabel});
            this.StatusStrip.Location = new System.Drawing.Point(0, 73);
            this.StatusStrip.Name = "StatusStrip";
            this.StatusStrip.Size = new System.Drawing.Size(875, 24);
            this.StatusStrip.TabIndex = 3;
            this.StatusStrip.Text = "statusStrip1";
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(875, 97);
            this.Controls.Add(this.StatusStrip);
            this.Controls.Add(this.groupBox1);
            this.Name = "Form2";
            this.Text = "Facebook posts download";
            this.Load += new System.EventHandler(this.Form2_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.StatusStrip.ResumeLayout(false);
            this.StatusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button DownloadButton;
        private System.Windows.Forms.DateTimePicker UntilDateTimePicker;
        private System.Windows.Forms.DateTimePicker SinceDateTimePicker;
        private System.Windows.Forms.CheckBox UntilCheckBox;
        private System.Windows.Forms.CheckBox SinceCheckBox;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.ToolStripStatusLabel AccessTokenStatusLabel;
        private System.Windows.Forms.StatusStrip StatusStrip;
    }
}