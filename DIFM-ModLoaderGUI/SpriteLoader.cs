using System;
using System.Windows.Forms;
using System.Net;
using System.Runtime.InteropServices;
using System.IO;
using Octokit;
using System.Linq;
using System.Drawing;


namespace DIFM_ModLoaderGUI
{
    public partial class SpriteLoader : Form
    {
        public SpriteLoader()
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

        private void SpriteLoader_Load(object sender, EventArgs e)
        {

        }
    }
}
