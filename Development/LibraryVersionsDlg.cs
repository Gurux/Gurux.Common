#if !NETCOREAPP2_0 && !NETSTANDARD2_0 && !NETSTANDARD2_1 && !NETCOREAPP2_1 && !NETCOREAPP3_1
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Win32;
using Gurux.Common.Properties;
using System.Reflection;

namespace Gurux.Common
{
    /// <summary>
    /// Show library versions.
    /// </summary>
    public partial class LibraryVersionsDlg : Form
    {
        /// <summary>
        /// Initializes a new instance of the LibraryVersionsDlg class.
        /// </summary>
        public LibraryVersionsDlg()
        {
            InitializeComponent();
            UpdateResources();
        }

        private void UpdateResources()
        {
            try
            {
                this.LocationHeader.Text = Resources.LocationTxt;
                this.CancelBtn.Text = Resources.CancelTxt;
                this.NameHeader.Text = Resources.NameTxt;
                this.VersionHeader.Text = Resources.VersionTxt;
                this.CopyBtn.Text = Resources.CopyTxt;
                this.Text = Resources.LibraryVersionsTxt;
                //Update help strings from the resource.
                this.helpProvider1.SetHelpString(this.listView1, Resources.LibraryListHelp);
                this.helpProvider1.SetHelpString(this.CopyBtn, Resources.CopyTxt);
                this.helpProvider1.SetHelpString(this.CancelBtn, Resources.ClosesTheDialogBoxWithoutSavingAnyChangesYouHaveMade);
            }
            catch (Exception Ex)
            {
                GXCommon.ShowError(Ex);
            }
        }

        private void LibraryVersionsDlg_Load(object sender, System.EventArgs e)
        {
            ListViewItem it;
            try
            {
                //Is .Net 3.5 installed.
                const string net35 = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v3.5";
                using (RegistryKey subKey = Registry.LocalMachine.OpenSubKey(net35))
                {
                    if (subKey != null && Convert.ToUInt32(subKey.GetValue("Install")) == 1)
                    {
                        string version = Convert.ToString(subKey.GetValue("Version"));
                        string servicePack = Convert.ToString(subKey.GetValue("SP"));
                        string str = Resources.NETFramework35;
                        if (!string.IsNullOrEmpty(servicePack))
                        {
                            str += Resources.SP + servicePack;
                        }
                        it = listView1.Items.Add(str);
                        it.SubItems.Add(version);
                        it.SubItems.Add(Convert.ToString(subKey.GetValue("InstallPath")));
                    }
                }
                //Is .Net 4.0 client installed.
                const string net40 = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Client";
                using (RegistryKey subKey = Registry.LocalMachine.OpenSubKey(net40))
                {
                    if (subKey != null && Convert.ToUInt32(subKey.GetValue("Install")) == 1)
                    {
                        string version = Convert.ToString(subKey.GetValue("Version"));
                        string servicePack = Convert.ToString(subKey.GetValue("SP"));
                        string str = Resources.NETFramework40;
                        if (!string.IsNullOrEmpty(servicePack))
                        {
                            str += Resources.SP + servicePack;
                        }
                        it = listView1.Items.Add(str);
                        it.SubItems.Add(version);
                        it.SubItems.Add(Convert.ToString(subKey.GetValue("InstallPath")));
                    }
                }
            }
            catch (Exception)
            {
                //Ignore errors.
            }
            List<string> assemblies = new List<string>();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    if (!GXCommon.IsDefaultAssembly(assembly))
                    {
                        string name = assembly.GetName().Name;
                        string ver = assembly.GetName().Version.ToString();
                        string loc = assembly.Location;
                        it = listView1.Items.Add(name);
                        it.SubItems.Add(ver);
                        it.SubItems.Add(loc);
                    }
                }
                catch (Exception)
                {
                    //Ignore errors.
                }
            }
        }

        private void listView1_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.C && e.Control)
            {
                CopyText();
            }
        }

        private void CopyText()
        {
            string data = string.Empty;
            foreach (ListViewItem it in listView1.Items)
            {
                data += it.Text + "\t" + it.SubItems[1].Text + "\t" + it.SubItems[2].Text + Environment.NewLine;
            }
            ClipboardCopy.CopyDataToClipboard(data);
        }

        private void CopyBtn_Click(object sender, System.EventArgs e)
        {
            CopyText();
        }

        /// <summary>
        /// Show help not available message.
        /// </summary>
        /// <param name="hevent">A HelpEventArgs that contains the event data.</param>
        protected override void OnHelpRequested(HelpEventArgs hevent)
        {
            // Get the control where the user clicked
            Control ctl = this.GetChildAtPoint(this.PointToClient(hevent.MousePos));
            string str = Resources.HelpNotAvailable;
            // Show as a Help pop-up
            if (str != "")
            {
                Help.ShowPopup(ctl, str, hevent.MousePos);
            }
            // Set flag to show that the Help event as been handled
            hevent.Handled = true;
        }
    }
}
#endif //!NETCOREAPP2_0 && !NETSTANDARD2_0 && !NETSTANDARD2_1 && !NETCOREAPP2_1 && !NETCOREAPP3_1