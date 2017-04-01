using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;

namespace BingWallpaper
{
    [SuppressMessage("ReSharper", "LocalizableElement")]
    internal class BingImage
    {
        private const string RequestUrl = "http://www.bing.com/HPImageArchive.aspx?format=js&idx={0}&n=1&mkt={1}";
        private static readonly WebClient WebClient = new WebClient {Encoding = Encoding.UTF8};

        private readonly string _url;

        private BingImage(string url, string description, int date)
        {
            _url = url;
            Description = description;
            Date = date;
        }

        public string Description { get; }
        public int Date { get; }

        public void WriteTo(string fileName)
        {
            var folder = Path.GetDirectoryName(fileName);

            if (folder == null) throw new ArgumentException("no path root of file path", nameof(fileName));

            Directory.CreateDirectory(folder);

            try
            {
                WebClient.DownloadFile(_url, fileName);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public static BingImage GetFromWeb(int dayOffset)
        {
            try
            {
                var langCode = CultureInfo.CurrentUICulture.Name;
                var json = JObject.Parse(WebClient.DownloadString(string.Format(RequestUrl, dayOffset, langCode)));
                var image = json["images"][0];

                var dateString = (string) image["startdate"];
                int date;
                if (!int.TryParse(dateString, out date)) return null;

                var url = "https://bing.com/" + ((bool) image["wp"] ? "hpwp/" + image["hsh"] : image["url"]);
                var copyright = (string) image["copyright"] ?? "";
                return new BingImage(url, copyright, date);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}