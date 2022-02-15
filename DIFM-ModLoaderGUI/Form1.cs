using System;
using System.Windows.Forms;
using System.Net;
using System.Runtime.InteropServices;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;
using Octokit;
using System.Linq;
using System.Drawing;
using DiscordRPC;
using DiscordRPC.Logging;

namespace DIFM_ModLoaderGUI
{
    public partial class Form1 : Form
    {
        public string downloadPath;
        public bool hasSet;
        public string actualURL;
        public int checkboxPressCount;
        public bool isDisablePostProccessingChecked;
        public DiscordRpcClient client;

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
            Initialize();
        }
        static async void GetReleases(ComboBox comboBox)
        {
            try
            {
                var client = new GitHubClient(new ProductHeaderValue("DiFM-Speedrun-Mod"));
                var releases = await client.Repository.Release.GetAll("LukeSaward1", "DiFM-Speedrun-Mod");
                var latest = releases[0];
                Array releaseArray = releases.ToArray();
                comboBox.Items.Add("Latest");
                foreach (var release in releases)
                {
                    comboBox.Items.Add(release.Name);
                    Console.WriteLine(release.Name);
                }
                if(comboBox.Items.Count >= releaseArray.Length)
                {
                    string msg = "Got " + comboBox.Items.Count + " releases.";
                    Console.WriteLine(msg);
                    MessageBox.Show(msg);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        //Called when your application first starts.
        //For example, just before your main loop, on OnEnable for unity.
        void Initialize()
        {
            /*
            Create a Discord client
            NOTE: 	If you are using Unity3D, you must use the full constructor and define
                     the pipe connection.
            */
            client = new DiscordRpcClient("927610658449666048");

            //Set the logger
            client.Logger = new ConsoleLogger() { Level = LogLevel.Warning };

            //Subscribe to events
            client.OnReady += (sender, e) =>
            {
                Console.WriteLine("Received Ready from user {0}", e.User.Username);
            };

            client.OnPresenceUpdate += (sender, e) =>
            {
                Console.WriteLine("Received Update! {0}", e.Presence);
            };

            //Connect to the RPC
            client.Initialize();

            //Set the rich presence
            //Call this as many times as you want and anywhere in your code.
            client.SetPresence(new RichPresence()
            {
                Details = "Example Project",
                State = "csharp example",
                Assets = new Assets()
                {
                    LargeImageKey = "heartpickup_0",
                    LargeImageText = "Lachee's Discord IPC Library"
                }
            });
        }
        private void Form1_Load(object sender, EventArgs e, Form1 form1)
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
                string fileExist = folderDlg.SelectedPath + @"\Do It For Me V1.0.1_Data\Managed\Assembly-CSharp.dll";
                if (File.Exists(fileExist))
                {
                    hasSet = true;
                    textBox1.Text = folderDlg.SelectedPath;
                    downloadPath = textBox1.Text + @"\Do It For Me V1.0.1_Data\Managed\Assembly-CSharp.dll";
                    Console.WriteLine("Getting releases...");
                    GetReleases(comboBox1);
                    Console.WriteLine("Got releases...");
                }
                else
                {
                    MessageBox.Show("Please select a valid game path.", "Invalid Game Path");
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (hasSet == true)
            {
                using (var client = new WebClient())
                {
                    downloadPath = textBox1.Text + @"\Do It For Me V1.0.1_Data\Managed\Assembly-CSharp.dll";
                    if (comboBox1.SelectedItem.ToString() == "Latest")
                    {
                        if (!isDisablePostProccessingChecked)
                        {
                            client.DownloadFile("https://raw.githubusercontent.com/LukeSaward1/DiFM-Speedrun-Mod/main/Assembly-CSharp.dll",
                                downloadPath);
                            MessageBox.Show("The file has been successfully downloaded and is located in the 'Managed' folder in the 'Do It For Me "
                                + "V1.0.1_Data' folder under Assembly-CSharp.dll", "File successfully downloaded");
                        }
                        else
                        {
                            client.DownloadFile("https://raw.githubusercontent.com/LukeSaward1/DiFM-Speedrun-Mod/main/Assembly-CSharp-NoPost.dll",
                                downloadPath);
                            MessageBox.Show("The file has been successfully downloaded and is located in the 'Managed' folder in the 'Do It For Me "
                                + "V1.0.1_Data' folder under Assembly-CSharp.dll", "File successfully downloaded");
                        }
                    }   
                    else
                    {
                        if(!isDisablePostProccessingChecked)
                        {
                            actualURL = "https://github.com/LukeSaward1/DiFM-Speedrun-Mod/releases/download/" + comboBox1.SelectedItem.ToString() +
                                "/Assembly-CSharp.dll";
                            client.DownloadFile(actualURL, downloadPath);
                            MessageBox.Show("The file has been successfully downloaded and is located in the 'Managed' folder in the 'Do It For Me " +
                                "V1.0.1_Data' folder under Assembly-CSharp.dll", "File successfully downloaded");
                        }
                        else
                        {
                            actualURL = "https://github.com/LukeSaward1/DiFM-Speedrun-Mod/releases/download/" + comboBox1.SelectedItem.ToString() +
                                "/Assembly-CSharp-NoPost.dll";
                            client.DownloadFile(actualURL, downloadPath);
                            MessageBox.Show("The file has been successfully downloaded and is located in the 'Managed' folder in the 'Do It For Me " +
                                "V1.0.1_Data' folder under Assembly-CSharp.dll", "File successfully downloaded");
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
                    MessageBox.Show("Now downloading unmodded game. Hang tight.");
                    client.DownloadFile("https://github.com/LukeSaward1/file-hosting/releases/download/DiFM_V1.0.1_64bit/DiFM_V1.0.1_64bit.zip", 
                        gameLocationPath);
                    ExtractZipContent(
                        gameLocationPath,
                        null,
                        extractedLocationPath
                    );
                    File.Delete(gameLocationPath);
                    MessageBox.Show("The game has been successfully downloaded into " + extractedLocationPath + ".", "Game successfully downloaded");
                    textBox1.Text = extractedLocationPath;
                    hasSet = true;
                }
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if(checkboxPressCount == 0 && !isDisablePostProccessingChecked)
            {
                DialogResult dialogResult = MessageBox.Show("This option disables all post-processing effects present in the game (grain, vignette, " +
                    "bloom, ambient occlusion etc.) PERMANENTLY. This option is not preferred as the effects make the game feel more authentic rather " +
                    "than the effects being modded out which makes it less authentic, unless you or whoever is playing the game find the effects too " +
                    "distracting or you/they have a sensory issue. \n\nDo you wish to disable post-processing effects?", "Confirm disable?",
                    MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    isDisablePostProccessingChecked = true;
                    checkBox1.Checked = true;
                    checkBox1.CheckState = CheckState.Checked;
                    checkboxPressCount += 1;
                }
                if (dialogResult == DialogResult.No)
                {
                    isDisablePostProccessingChecked = false;
                    checkBox1.Checked = false;
                    checkBox1.CheckState = CheckState.Unchecked;
                }
            }
            else
            {
                isDisablePostProccessingChecked = !isDisablePostProccessingChecked;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            SpriteLoader sprLoaderFrm = new SpriteLoader();
            sprLoaderFrm.Show();
        }
    }
}