using System.Reflection;
using System.Windows.Forms;
using System;

namespace BingWallpaper
{
    internal static class StaticExtensionMethods
    {
        private static readonly MethodInfo NotifyIconShowContextMenuMethod =
            typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);

        public static void ShowContextMenu(this NotifyIcon notifyIcon)
        {
            NotifyIconShowContextMenuMethod.Invoke(notifyIcon, null);
        }

        public static int Marquee(this int i, int lowerBound, int upperBound)
        {
            if (upperBound < lowerBound) throw new ArgumentException("must be greater than " + nameof(lowerBound), nameof(upperBound));

            if (i < lowerBound) return upperBound;
            if (i > upperBound) return lowerBound;

            return i;
        }
    }
}