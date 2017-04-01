using BingWallpaper.Properties;
using System;
using System.IO;

namespace BingWallpaper
{
    internal class Watcher
    {
        private const int IMG_COUNT = 14;
        private const int IMG_LOWER_BOUND = 0, IMG_UPPER_BOUND = IMG_COUNT - 1;

        private static readonly string ImageFile = Path.Combine(
              Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
              AssemblyUtil.ProductName(), "Wallpaper.jpg");

        private readonly Action<String> _descriptionTextSetter, _indexTextSetter;

        private int _imageOffset = IMG_LOWER_BOUND;
        private BingImage _currentImage = null;

        public int ImageOffset => _imageOffset;

        internal Watcher(Action<String> descriptionTextSetter, Action<String> indexTextSetter)
        {
            _descriptionTextSetter = descriptionTextSetter;
            _indexTextSetter = indexTextSetter;
        }

        public void Tick()
        {
            var newImage = BingImage.GetFromWeb();

            if (newImage == null) return;
            if (_currentImage != null && _currentImage.Date > newImage.Date) return;

            _currentImage = newImage;
            _imageOffset = IMG_LOWER_BOUND;

            SetDesktopBackground(_currentImage);
        }

        public void ChangeImage(int newOffset)
        {
            _imageOffset = newOffset.Marquee(IMG_LOWER_BOUND, IMG_UPPER_BOUND);

            var img = BingImage.GetFromWeb(_imageOffset);
            if (img != null)
                SetDesktopBackground(img);
        }

        private void SetDesktopBackground(BingImage image)
        {
            image.WriteTo(ImageFile);

            if (!NativeMethods.SetWallpaper(ImageFile))
                return;

            var wrappedText = image.Description.WordWrap(50);
            _descriptionTextSetter(wrappedText);
            _indexTextSetter(Resources.Program_UseImage_Index + ": " + (_imageOffset + 1) + " / " + IMG_COUNT);
        }
    }
}
