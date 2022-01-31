using System;
using System.Windows.Forms;
using System.Net;
using System.Runtime.InteropServices;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;

namespace DIFM_ModLoaderGUI
{
    public partial class Form1 : Form
    {
        public string downloadPath;
        public bool hasSet;
        public string actualURL;
        public Form1()
        {
            InitializeComponent();
        }

        [DllImport("DwmApi")] //System.Runtime.InteropServices
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, int[] attrValue, int attrSize);

        protected override void OnHandleCreated(EventArgs e)
        {
            if (DwmSetWindowAttribute(Handle, 19, new[] { 1 }, 4) != 0)
                DwmSetWindowAttribute(Handle, 20, new[] { 1 }, 4);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Extracts the content from a .zip file inside an specific folder.
        /// </summary>
        /// <param name="FileZipPath"></param>
        /// <param name="password"></param>
        /// <param name="OutputFolder"></param>
        public void ExtractZipContent(string FileZipPath, string password, string OutputFolder)
        {
            ZipFile file = null;
            try
            {
                FileStream fs = File.OpenRead(FileZipPath);
                file = new ZipFile(fs);

                if (!String.IsNullOrEmpty(password))
                {
                    // AES encrypted entries are handled automatically
                    file.Password = password;
                }

                foreach (ZipEntry zipEntry in file)
                {
                    if (!zipEntry.IsFile)
                    {
                        // Ignore directories
                        continue;
                    }

                    String entryFileName = zipEntry.Name;
                    // to remove the folder from the entry:- entryFileName = Path.GetFileName(entryFileName);
                    // Optionally match entrynames against a selection list here to skip as desired.
                    // The unpacked length is available in the zipEntry.Size property.

                    // 4K is optimum
                    byte[] buffer = new byte[4096];
                    Stream zipStream = file.GetInputStream(zipEntry);

                    // Manipulate the output filename here as desired.
                    String fullZipToPath = Path.Combine(OutputFolder, entryFileName);
                    string directoryName = Path.GetDirectoryName(fullZipToPath);

                    if (directoryName.Length > 0)
                    {
                        Directory.CreateDirectory(directoryName);
                    }

                    // Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
                    // of the file, but does not waste memory.
                    // The "using" will close the stream even if an exception occurs.
                    using (FileStream streamWriter = File.Create(fullZipToPath))
                    {
                        StreamUtils.Copy(zipStream, streamWriter, buffer);
                    }
                }
            }
            finally
            {
                if (file != null)
                {
                    file.IsStreamOwner = true; // Makes close also shut the underlying stream
                    file.Close(); // Ensure we release resources
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderDlg = new FolderBrowserDialog();
            DialogResult result = folderDlg.ShowDialog();

            if (result == DialogResult.OK)
            {
                hasSet = true;
                textBox1.Text = folderDlg.SelectedPath;
                downloadPath = textBox1.Text + @"\Do It For Me V1.0.1_Data\Managed\Assembly-CSharp.dll";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (hasSet == true)
            {
                using (var client = new WebClient())
                {
                    downloadPath = textBox1.Text + @"\Do It For Me V1.0.1_Data\Managed\Assembly-CSharp.dll";
                    if (textBox2.Text.Contains("Latest") || textBox2.Text.Contains("latest"))
                    {
                        try
                        {
                            client.DownloadFile("https://raw.githubusercontent.com/LukeSaward1/DiFM-Speedrun-Mod/main/Assembly-CSharp.dll", downloadPath);
                            MessageBox.Show("The file has been successfully downloaded and is located in the 'Managed' folder in the 'Do It For Me V1.0.1_Data' " +
                                "folder under Assembly-CSharp.dll", "File successfully downloaded");
                        }
                        catch
                        {
                            MessageBox.Show("You must enter a valid version number (e.g. 1.0.2.1 or latest)");
                        }
                    }
                    else
                    {
                        try
                        {
                            actualURL = "https://github.com/LukeSaward1/DiFM-Speedrun-Mod/releases/download/v" + textBox2.Text + "/Assembly-CSharp.dll";
                            client.DownloadFile(actualURL, downloadPath);
                            MessageBox.Show("The file has been successfully downloaded and is located in the 'Managed' folder in the 'Do It For Me V1.0.1_Data' " +
                                "folder under Assembly-CSharp.dll", "File successfully downloaded");
                        }
                        catch
                        {
                            MessageBox.Show("You must enter a valid version number (e.g. 1.0.2.1 or latest)");
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a game directory.");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Please select the location where you wish to store the game.", "Select Game Location");
            FolderBrowserDialog gameLocationDlg = new FolderBrowserDialog();
            DialogResult result = gameLocationDlg.ShowDialog();
            if (result == DialogResult.OK)
            {
                string gameLocationPath = gameLocationDlg.SelectedPath + @"\DiFM_V1.0.1_64bit.zip";
                string gameFileLocationPath = gameLocationDlg.SelectedPath;
                string extractedLocationPath = gameLocationDlg.SelectedPath + @"\DoItForMe_V1.0.1_64bit";
                using (var client = new WebClient())
                {
                    MessageBox.Show("Now downloading game. Hang tight.");
                    client.DownloadFile("https://github.com/LukeSaward1/file-hosting/releases/download/DiFM_V1.0.1_64bit/DiFM_V1.0.1_64bit.zip", gameLocationPath);
                    ExtractZipContent(
                        gameLocationPath,
                        null,
                        extractedLocationPath
                    );
                    MessageBox.Show("The game has been successfully downloaded into " + extractedLocationPath + ".", "Game successfully downloaded");
                    textBox1.Text = extractedLocationPath;
                    hasSet = true;
                }
            }
        }
    }
}