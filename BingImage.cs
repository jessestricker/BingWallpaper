using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Text;
using System.Xml.Linq;

namespace BingWallpaper
{
    [SuppressMessage("ReSharper", "LocalizableElement")]
    internal class BingImage
    {
        private const string RequestUrl = "http://www.bing.com/HPImageArchive.aspx?format=xml&idx={0}&n=1&mkt=de-DE";
        private static readonly WebClient WebClient = new WebClient();

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

            if (folder == null)
                throw new ArgumentException("no path root of file path", nameof(fileName));

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

        public static BingImage GetFromWeb(int dayOffset = -1)
        {
            string xml;
            try
            {
                xml = WebClient.DownloadString(string.Format(RequestUrl, dayOffset));
            }
            catch (Exception)
            {
                return null;
            }

            XDocument xmlRoot;
            try
            {
                xmlRoot = XDocument.Parse(xml);
            }
            catch (Exception)
            {
                return null;
            }

            var xmlImages = xmlRoot.Element("images");
            var xmlImage = xmlImages?.Element("image");

            var dateString = xmlImage?.Element("startdate")?.Value;
            int date;

            if (!int.TryParse(dateString, out date))
                return null;

            var url = "http://bing.com" + xmlImage?.Element("url")?.Value;
            var description = xmlImage?.Element("copyright")?.Value;

            if (description == null)
                return null;

            // utf8 -> native encoding
            description = Encoding.UTF8.GetString(Encoding.Default.GetBytes(description));

            return new BingImage(url, description, date);
        }
    }
}