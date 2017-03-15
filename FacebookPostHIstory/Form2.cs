using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using PostRetriever;

namespace FacebookPostHistory
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        public string AccessToken { private get; set; }

        private void Form2_Load(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(AccessToken))
            {
                AccessTokenStatusLabel.Text = @"Access token loaded!";
                AccessTokenStatusLabel.BackColor = Color.Green;
                AccessTokenStatusLabel.ForeColor = Color.White;
                AccessTokenStatusLabel.Font = new Font(AccessTokenStatusLabel.Font, FontStyle.Bold);
            }
        }

        private void SinceCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SinceDateTimePicker.Enabled = SinceCheckBox.Checked;
        }

        private void UntilCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            UntilDateTimePicker.Enabled = UntilCheckBox.Checked;
        }

        private void DownloadButton_Click(object sender, EventArgs e)
        {
            if (SinceCheckBox.Checked && UntilCheckBox.Checked)
            {
                saveFileDialog1.Filter = @"Excel files (*.xlsx; *xls) | *.xlsx; *xls;";
                saveFileDialog1.ShowDialog();

                if (!string.IsNullOrEmpty(saveFileDialog1.FileName))
                {
                    var c = new ConsoleProgram();
                    Process.Start(c.ReturnPath(),
                        $"Hello {AccessToken} \"{saveFileDialog1.FileName}\" \"{SinceDateTimePicker.Value}\" \"{UntilDateTimePicker.Value}\"");
                    Close();
                }
                else
                {
                    MessageBox.Show(@"Please input a destination file!", @"Error!", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show(@"Please select a time frame to download posts", @"Error!", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}