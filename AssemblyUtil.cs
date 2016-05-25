using System.Reflection;

namespace BingWallpaper
{
    internal static class AssemblyUtil
    {
        private static Assembly CurrentAssembly => Assembly.GetExecutingAssembly();

        public static string ProductName(Assembly assembly = null)
            => (assembly ?? CurrentAssembly).GetCustomAttribute<AssemblyProductAttribute>().Product;

        public static string CompanyName(Assembly assembly = null)
            => (assembly ?? CurrentAssembly).GetCustomAttribute<AssemblyCompanyAttribute>().Company;
    }
}