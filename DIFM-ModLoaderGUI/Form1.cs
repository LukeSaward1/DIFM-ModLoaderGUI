using System;
using System.Windows.Forms;
using System.Net;

namespace DIFM_ModLoaderGUI
{
    public partial class Form1 : Form
    {
        public string downloadPath;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderDlg = new FolderBrowserDialog();
            DialogResult result = folderDlg.ShowDialog();
            if (result == DialogResult.OK)
            {
                downloadPath = folderDlg.SelectedPath + @"\Do It For Me V1.0.1_Data\Managed\Assembly-CSharp.dll";
                textBox1.Text = folderDlg.SelectedPath;
                Environment.SpecialFolder root = folderDlg.RootFolder;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (var client = new WebClient())
            {
                if(textBox2.Text.Contains("Latest") || textBox2.Text.Contains("latest"))
                {
                    client.DownloadFile("https://raw.githubusercontent.com/LukeSaward1/DiFM-Speedrun-Mod/main/Assembly-CSharp.dll", downloadPath);
                }
                else
                {
                    string actualURL = "https://github.com/LukeSaward1/DiFM-Speedrun-Mod/releases/download/v" + textBox2.Text + "/Assembly-CSharp.dll";
                    client.DownloadFile(actualURL, downloadPath);
                }
                MessageBox.Show("The file has been successfully downloaded and is located in the 'Managed' folder in the 'Do It For Me V1.0.1_Data' folder under Assembly-CSharp.dll", "File successfully downloaded");
            }
        }
    }
}
