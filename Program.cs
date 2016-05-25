using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using BingWallpaper.Properties;
using Microsoft.Win32;

namespace BingWallpaper
{
    [SuppressMessage("ReSharper", "LocalizableElement")]
    internal class Program : ApplicationContext
    {
        private static readonly string ImageFile =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                AssemblyUtil.ProductName(), "Wallpaper.jpg");

        private static readonly string InstallationFolder =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                AssemblyUtil.CompanyName(), AssemblyUtil.ProductName());

        private static readonly string InstallationExecutablePath =
            Path.ChangeExtension(
                Path.Combine(InstallationFolder, AssemblyUtil.ProductName()),
                "exe");

        private static readonly RegistryKey RunAtStartupRegistryKey =
            Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");

        private static readonly Mutex SiMutex = new Mutex(true, "68592096-BFAB-46DB-9B7F-DE977678D85E");

        private readonly ToolStripLabel _currentImageLabel, _currentIndexLabel;
        private readonly NotifyIcon _notifyIcon;
        private readonly Thread _watchThread;

        private int _imageOffset = -1;

        private Program()
        {
            ThreadExit += OnProgramExit;

            _notifyIcon = new NotifyIcon
            {
                Text = AssemblyUtil.ProductName(),
                Icon = Resources.Icon,
                Visible = true,
                ContextMenuStrip = new ContextMenuStrip
                {
                    Items =
                    {
                        (_currentImageLabel = new ToolStripLabel()),
                        (_currentIndexLabel = new ToolStripLabel()),
                        new ToolStripSeparator(),
                        new ToolStripButton("Vorheriges Bild", null, OnPreviousImageClick),
                        new ToolStripButton("Nächstes Bild", null, OnNextImageClick)
                    }
                }
            };

            _currentImageLabel.Font = new Font(_currentImageLabel.Font, FontStyle.Bold);

            // open context menu strip on click
            _notifyIcon.Click += (sender, args) => _notifyIcon.ShowContextMenu();

            // start watching
            _watchThread = new Thread(WatchThread);
            _watchThread.Start();
        }

        private void ChangeImage(int newOffset)
        {
            _imageOffset = newOffset.Marquee(-1, 12);

            var img = BingImage.GetFromWeb(_imageOffset);
            if (img != null)
                UseImage(img);
        }

        private void UseImage(BingImage image)
        {
            image.WriteTo(ImageFile);

            if (!NativeMethods.SetWallpaper(ImageFile))
                return;

            var wrappedText = image.Description.WordWrap(50);
            _currentImageLabel.Text = wrappedText;
            _currentIndexLabel.Text = "Bildindex: " + (_imageOffset + 2) + " / 14";
        }

        private void OnPreviousImageClick(object sender, EventArgs eventArgs)
        {
            ChangeImage(_imageOffset + 1);
        }

        private void OnNextImageClick(object sender, EventArgs eventArgs)
        {
            ChangeImage(_imageOffset - 1);
        }

        private void OnProgramExit(object sender, EventArgs eventArgs)
        {
            _watchThread.Abort();
            _notifyIcon.Dispose();
        }

        [SuppressMessage("ReSharper", "FunctionNeverReturns")]
        private void WatchThread()
        {
            BingImage currentImage = null;
            while (true)
            {
                var newImage = BingImage.GetFromWeb();

                if (newImage != null &&
                    (currentImage == null || newImage.Date > currentImage.Date))
                {
                    currentImage = newImage;
                    _imageOffset = -1;
                    UseImage(currentImage);
                }

                Thread.Sleep(TimeSpan.FromMinutes(30));
            }
        }

        [STAThread]
        private static void Main()
        {
            if (Environment.GetCommandLineArgs().Contains("--uninstall"))
            {
                Uninstall();
                return;
            }


            // allow only one instance if not debugging
#if !DEBUG
            if (!SiMutex.WaitOne(TimeSpan.Zero, true))
            {
                Console.WriteLine("One instance is already running, Quitting!");
                return;
            }
#endif

            if (!Install())
                return;

            // run program
            Application.Run(new Program());

#if !DEBUG
            SiMutex.ReleaseMutex();
#endif
        }

        private static bool Install()
        {
            if (Environment.GetCommandLineArgs().Contains("--no-install"))
                return true;

            var forceInstall = Environment.GetCommandLineArgs().Contains("--install");
            var registryRunValue = RunAtStartupRegistryKey.GetValue(AssemblyUtil.ProductName()) as string;

            if (!forceInstall && File.Exists(InstallationExecutablePath) &&
                registryRunValue == InstallationExecutablePath)
                return true;

            // install
            Console.Write("Installing...");
            try
            {
                // create installation folder
                Directory.CreateDirectory(InstallationFolder);
                File.Copy(Application.ExecutablePath, InstallationExecutablePath, true);

                // set up 'run on login'
                RunAtStartupRegistryKey.SetValue(AssemblyUtil.ProductName(), InstallationExecutablePath);

                Console.WriteLine("Success");
                return true;
            }
            catch (Exception e)
            {
                var message = "Failed to install!\n\n" + e.Message;

                if (e is UnauthorizedAccessException)
                {
                    message += "\n\nRestart this program as an administrator to install!";
                }

                MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Console.WriteLine("Failed, see MessageBox");
                return false;
            }
        }

        private static void Uninstall()
        {
            Console.Write("Uninstalling...");

            try
            {
                RunAtStartupRegistryKey.DeleteValue(AssemblyUtil.ProductName(), false);
                Directory.Delete(InstallationFolder, true);

                Console.WriteLine("Success");
            }
            catch (Exception e)
            {
                var message = "Failed to uninstall!\n\n" + e.Message;

                if (e is UnauthorizedAccessException)
                {
                    message += "\n\nRestart this program as an administrator to uninstall!";
                }

                MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                Console.WriteLine("Failed, restart as administrator");
            }

            Console.WriteLine("Quitting!");
        }
    }
}