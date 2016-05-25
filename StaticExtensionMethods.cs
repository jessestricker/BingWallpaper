using System.Reflection;
using System.Windows.Forms;

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

        public static int Marquee(this int i, int beg, int end)
        {
            if (i > end) i = beg;
            else if (i < beg) i = end;
            return i;
        }
    }
}