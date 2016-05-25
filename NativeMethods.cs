using System.Runtime.InteropServices;

namespace BingWallpaper
{
    internal static class NativeMethods
    {
        // SystemParametersInfo
        private const uint SpiSetDeskWallpaper = 20;
        private const uint SpifUpdateIniFile = 0x01;

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, string pvParam, uint fWinIni);

        // Accessors
        public static bool SetWallpaper(string file)
            => SystemParametersInfo(SpiSetDeskWallpaper, 0, file, SpifUpdateIniFile);
    }
}