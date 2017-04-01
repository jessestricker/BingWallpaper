using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("Bing Wallpaper")]
[assembly: AssemblyProduct("Bing Wallpaper")]
[assembly: AssemblyCompany("Jesse Stricker")]
[assembly: AssemblyCopyright("Copyright © 2016 Jesse Stricker")]
[assembly: AssemblyDescription("Bing Wallpaper downloads the current image from bing.com to show as the desktop background image.")]
[assembly: ComVisible(false)]
[assembly: AssemblyVersion("1.3.3")]

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif
