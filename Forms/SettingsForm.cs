using Microsoft.Win32;
using System;
using System.IO;
using System.Windows.Forms;

namespace BingWallpaper.Forms
{
    public partial class SettingsForm : Form
    {
        private static string GetExecutablePath()
        {
            var uri = new Uri(AssemblyUtil.ExecutablePath());
            var path = uri.LocalPath + Uri.UnescapeDataString(uri.Fragment);
            return Path.GetFullPath(path);
        }

        private static readonly string ExecutablePath = GetExecutablePath();
        private static RegistryKey OpenRunRegistryKey() => Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");

        private static bool IsRunAtStart()
        {
            var registryKey = OpenRunRegistryKey();
            var registryValueNames = registryKey.GetValueNames();
            foreach (var name in registryValueNames)
            {
                var value = registryKey.GetValue(name);
                if (value as string == ExecutablePath)
                    return true;
            }
            return false;
        }

        private static void SetRunAtStart(bool enabled)
        {
            var registryKey = OpenRunRegistryKey();
            var registryValueNames = registryKey.GetValueNames();

            if (!enabled)
            {
                // remove all values with installation path set
                foreach (var name in registryValueNames)
                {
                    var value = registryKey.GetValue(name);
                    if (value as string == ExecutablePath)
                        registryKey.DeleteValue(name);
                }
            }
            else
            {
                // set value if not set already
                foreach (var name in registryValueNames)
                {
                    var value = registryKey.GetValue(name);
                    if (value as string == ExecutablePath)
                        return;
                }
                registryKey.SetValue(AssemblyUtil.ProductName(), ExecutablePath);
            }
        }

        public SettingsForm()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            CheckBoxStartWithWindows.Checked = IsRunAtStart();
        }

        private void SaveSettings()
        {
            SetRunAtStart(CheckBoxStartWithWindows.Checked);
        }

        #region Events

        private void ButtonOk_Click(object sender, EventArgs e)
        {
            SaveSettings();
            Close();
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        #endregion
    }
}
