using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using BingWallpaper.Properties;
using BingWallpaper.Forms;

namespace BingWallpaper
{
    internal class Program : ApplicationContext
    {
        private static readonly string ImageFile =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                AssemblyUtil.ProductName(), "Wallpaper.jpg");

        private static readonly Mutex SiMutex = new Mutex(true, "68592096-BFAB-46DB-9B7F-DE977678D85E");

        private readonly ToolStripLabel _currentImageLabel, _currentIndexLabel;
        private readonly NotifyIcon _notifyIcon;
        private readonly Thread _watchThread;

        private int _imageOffset;
        private Form _settingsForm;

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
                        new ToolStripButton(Resources.Program_Program_Previous_Wallpapaper, null, OnPreviousImageClick),
                        new ToolStripButton(Resources.Program_Program_Next_Wallpaper, null, OnNextImageClick),
                        new ToolStripSeparator(),
                        new ToolStripButton(Resources.Program_Program_Settings, null, OnSettingsButtonClick),
                        new ToolStripButton(Resources.Program_Program_Exit, null, OnCloseButtonClick)
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

        private void OnSettingsButtonClick(object sender, EventArgs e)
        {
            if (_settingsForm == null || _settingsForm.IsDisposed)
            {
                // settings form invalid, create new one and show it
                _settingsForm = new SettingsForm();
                _settingsForm.Show();
            }
            else
            {
                // valid form opened, bring it back to front
                _settingsForm.Focus();
            }
        }

        private void OnCloseButtonClick(object sender, EventArgs e)
        {
            ExitThread();
        }

        private void ChangeImage(int newOffset)
        {
            _imageOffset = newOffset.Marquee(0, 13);

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
            _currentIndexLabel.Text = string.Format(Resources.Program_UseImage_Index, _imageOffset + 1);
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
                var newImage = BingImage.GetFromWeb(0);

                if (newImage != null &&
                    (currentImage == null || newImage.Date > currentImage.Date))
                {
                    currentImage = newImage;
                    _imageOffset = 0;
                    UseImage(currentImage);
                }

                Thread.Sleep(TimeSpan.FromMinutes(30));
            }
        }

        [STAThread]
        private static void Main()
        {
            // allow only one instance if not debugging
#if !DEBUG
            if (!SiMutex.WaitOne(TimeSpan.Zero, true))
            {
                Console.WriteLine("One instance is already running, Quitting!");
                return;
            }
#endif

            Application.EnableVisualStyles();

            // run program
            Application.Run(new Program());

#if !DEBUG
            SiMutex.ReleaseMutex();
#endif
        }
    }
}