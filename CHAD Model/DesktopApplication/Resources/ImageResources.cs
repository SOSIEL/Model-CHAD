using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DesktopApplication.Resources
{
    public static class ImageResources
    {
        #region Properties, Indexers

        public static ImageSource Logo =>
            new BitmapImage(new Uri("pack://application:,,,/Resources/SOSIEL.png", UriKind.RelativeOrAbsolute));

        #endregion
    }
}