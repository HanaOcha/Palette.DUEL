using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace palette.duel
{
    public partial class DropperWindow : Window
    {
        public MainWindow mainWindow;

        public byte[] pixels = new byte[256];
        int stride = 0;
        FormatConvertedBitmap bitmap;
        public DropperWindow(MainWindow mainWindow, string source)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;

            this.ResizeMode = ResizeMode.CanMinimize;

            using (FileStream stream = File.OpenRead(source))
            {
                Console.WriteLine(source);
                BitmapImage imageMap = new BitmapImage();
                imageMap.BeginInit();
                imageMap.StreamSource = stream; imageMap.CacheOption = BitmapCacheOption.OnLoad;
                imageMap.EndInit();

                BitmapPalette bitmapPalette = new BitmapPalette(imageMap, 256);
                this.bitmap = new FormatConvertedBitmap(imageMap, PixelFormats.Bgra32, bitmapPalette, 0);

                this.stride = (bitmap.PixelWidth * bitmap.Format.BitsPerPixel + 7) / 8;
                this.pixels = new byte[stride * bitmap.PixelHeight];
                this.bitmap.CopyPixels(pixels, stride, 0);

                this.dropperImage.Source = bitmap;
            }

            this.scrollHorizontal.Visibility = Visibility.Hidden;
            this.scrollHorizontal.IsEnabled = false;
            this.scrollVertical.Visibility = Visibility.Hidden;
            this.scrollVertical.IsEnabled = false;

            if (bitmap.PixelHeight > this.Height)
            {
                this.scrollVertical.Visibility = Visibility.Visible;
                this.scrollVertical.IsEnabled = true;
                this.scrollVertical.Maximum = bitmap.PixelHeight - (int)this.Height;
            }
            if (bitmap.PixelWidth > this.Width)
            {
                this.scrollHorizontal.Visibility = Visibility.Visible;
                this.scrollHorizontal.IsEnabled = true;
                this.scrollHorizontal.Maximum = bitmap.PixelWidth - (int)this.Width;
            }
            this.Scroll(null, null);
        }
        public void Scroll(object? o, EventArgs? e)
        {
            int x = (int)this.scrollHorizontal.Value;
            int y = (int)this.scrollVertical.Value;

            Canvas.SetLeft(this.dropperImage, -x);
            Canvas.SetTop(this.dropperImage, -y);
        }

        public bool GetPixelAtPointer(out Color color)
        {
            Point mouse = Mouse.GetPosition(this.dropperImage);
            int x = (int)MathF.Round((float)mouse.X);
            int y = (int)MathF.Round((float)mouse.Y);
            int i = y * this.bitmap.PixelWidth + x;
            int j = i * 4;

            if (this.pixels.Length > j + 3)
            {
                color = Color.FromArgb(this.pixels[j + 3], this.pixels[j + 2], this.pixels[j + 1], this.pixels[j]);
                return true;
            }

            return false;
        }
        public void ClickImage(object? o, EventArgs? e)
        {
            if (this.GetPixelAtPointer(out Color color) && this.mainWindow != null)
            {
                this.mainWindow.paletteEditor.SetSelectorColor(color);
            }
        }
    }
}
