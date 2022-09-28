using Devdeb.Images.CanonRaw.Drawing;
using Devdeb.Images.CanonRaw.FileStructure;
using Devdeb.Serialization.Serializers.System;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Devdeb.Images.Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CannonRaw3 _cannonRaw3;
        private Pixel42[,] _sourcePixels;
        private PixelConverter _pixelConverter;

        public MainWindow() => InitializeComponent();

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            LoadImage();
            RecalculateImage();
        }

        void LoadImage()
        {
            //IMG_6879
            using var fileStream = new FileStream(@"C:\Users\lehac\Desktop\IMG_6879.CR3", FileMode.Open, FileAccess.Read);
            byte[] fileMemory = new byte[fileStream.Length];

            fileStream.Read(fileMemory, 0, fileMemory.Length);

            var signature = ((Memory<byte>)fileMemory).Slice(4, 8);
            string someString = "ftypcrx ";
            byte[] someStringBuffer = new byte[StringSerializer.Default.Size(someString)];
            StringSerializer.Default.Serialize(someString, someStringBuffer, 0);
            if (signature.Span.SequenceEqual(someStringBuffer))
            {
                _cannonRaw3 = new(fileMemory);
                _sourcePixels = _cannonRaw3.ParseCrxHdImage();
                _pixelConverter = new PixelConverter(_cannonRaw3.MaxColorValue);
            }
        }

        void RecalculateImage()
        {
            byte[] imageBuffer = PixelsConvert.ToPixel24ByteArray(
                    _sourcePixels,
                    _pixelConverter.ConverPixels,
                    _cannonRaw3.ImageAreaSize,
                    _pixelConverter
                );
            BitmapSource bitmapSource = BitmapSource.Create(5568, 3706, 0, 0, PixelFormats.Rgb24, BitmapPalettes.WebPalette, imageBuffer, 16704);
            Image.Source = bitmapSource;
        }

        private void BlackPointSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _pixelConverter.BlackPointFactor = e.NewValue;
            RecalculateImage();
        }

        private void WhitePointSlizer_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _pixelConverter.WhitePointFactor = e.NewValue;
            RecalculateImage();
        }
    }
}
