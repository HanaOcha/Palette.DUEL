using Microsoft.Xaml.Behaviors.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace palette.duel
{
    internal class Data
    {
        public static List<Character> characters = new List<Character>();
        public static Dictionary<string, PaletteCount> paletteCounts = new Dictionary<string, PaletteCount>();

        internal class Path
        {
            public string I;
            public string A;
            public string splashes;
            public string sheets;
            public string palettes;
            internal Path()
            {
                I = AppDomain.CurrentDomain.BaseDirectory;
                A = I + "assets\\";
                splashes = A + "splashes\\";
                sheets = A + "sheets\\";
                palettes = A + "palettes\\";
            }
        }
        public static Path PATH = new Path();

        public static void ReadPaletteCounts()
        {
            string[] counts = File.ReadAllLines(PATH.A + "palette counts.txt");
            foreach (string s in counts)
            {
                string[] split = s.Split(":");
                string name = split[0];
                string[] data = split[1].Split(";");
                string[] sizes = data[0].Split(",");
                string[] preview = data[1].Split(",");
                
                PaletteCount count = new PaletteCount();
                count.count = new Vector2(int.Parse(sizes[0]), int.Parse(sizes[1]));
                count.size = new Vector2(int.Parse(sizes[2]), int.Parse(sizes[3]));
                count.preview = new Vector2(int.Parse(preview[0]), int.Parse(preview[1]));
                paletteCounts.Add(name, count);
            }
        }
        public static void ReadCharacters()
        {
            DirectoryInfo splashes = new DirectoryInfo(PATH.splashes);
            DirectoryInfo outfits = new DirectoryInfo(PATH.sheets);

            foreach (FileInfo splash in splashes.GetFiles())
            {
                Character character = new Character();
                character.name = splash.Name.Replace(".png", string.Empty);
                character.splash = PATH.splashes + character.name + ".png";

                foreach (FileInfo file in outfits.GetFiles())
                {
                    if (file.Name.StartsWith(character.name))
                    {
                        Outfit outfit = new Outfit();

                        outfit.name = file.Name.Replace(character.name + "_", string.Empty).Replace(".png", string.Empty);
                        outfit.sheet = PATH.sheets + character.name + "_" + outfit.name + ".png";
                        outfit.palette = PATH.palettes + character.name + "_" + outfit.name + ".png";
                        outfit.normal = PATH.palettes + character.name + "_" + outfit.name + "-G.png";
                        outfit.spriteData = paletteCounts[character.name + "_" + outfit.name];

                        character.outfits.Add(outfit);
                    }
                }

                characters.Add(character);
            }
        }

        public static Color GetPixel(BitmapSource bitmap, int x, int y)
        {
            if (bitmap.Format != PixelFormats.Bgra32)
            {
                bitmap = new FormatConvertedBitmap(bitmap, PixelFormats.Bgra32, null, 0);
            }

            // stride is the length of the data..? per row
            int stride = (bitmap.PixelWidth * bitmap.Format.BitsPerPixel + 7) / 8;
            byte[] pixels = new byte[stride * bitmap.PixelHeight];
            bitmap.CopyPixels(pixels, stride, 0);

            int i = (y * bitmap.PixelWidth + x) * 4;

            return Color.FromArgb(pixels[i + 3], pixels[i + 2], pixels[i + 1], pixels[i]);
        }
        public static void SetPixel(WriteableBitmap bitmap, int x, int y, Color color)
        {
            int stride = (bitmap.PixelWidth * bitmap.Format.BitsPerPixel + 7) / 8;
            byte[] pixels = new byte[stride * bitmap.PixelHeight];
            bitmap.CopyPixels(pixels, stride, 0);

            int i = (y * bitmap.PixelWidth + x) * 4;
            pixels[i] = color.B; 
            pixels[i + 1] = color.G; 
            pixels[i + 2] = color.R; 
            pixels[i + 3] = color.A;

            bitmap.WritePixels(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight), pixels, stride, 0);
        }
    }
    public class Character
    {
        public string name = string.Empty;
        public string splash = "";
        public List<Outfit> outfits = new List<Outfit>();
    }
    public class Outfit
    {
        public string name = string.Empty;
        public string sheet = "";
        public string palette = "";
        public string normal = "";
        public PaletteCount spriteData = new PaletteCount();
    }
    public class PaletteCount
    {
        public Vector2 count = new Vector2();
        public Vector2 size = new Vector2();
        public Vector2 preview = new Vector2();
        public int width()
        {
            return (int)count.X * (int)size.X;
        }
        public int height()
        {
            return (int)count.Y * (int)size.Y;
        }
        public int area()
        {
            return this.width() * this.height();
        }
    }

    internal class Shortcuts
    {
        public static void ReadKey(MainWindow window, Key key, ModifierKeys modifier)
        {
            if (window == null || window.paletteEditor == null || window.charSelect == null)
            {
                return;
            }

            if (window.characterSelectCanvas.Visibility == Visibility.Visible)
            {

            }
            else if (window.paletteEditCanvas.Visibility == Visibility.Visible)
            {
                if (key == Key.S && modifier == ModifierKeys.Control)
                {
                    window.paletteEditor.Export(null, null);
                    return;
                }

                if (key == Key.OemComma && modifier == ModifierKeys.Control)
                {
                    window.paletteEditor.FrameBackT(null, null);
                    return;
                }
                if (key == Key.OemPeriod && modifier == ModifierKeys.Control)
                {
                    window.paletteEditor.FrameForwardT(null, null);
                    return;
                }

                if (key == Key.F && modifier == ModifierKeys.Control)
                {
                    window.paletteEditor.NextFrameWithFocus(null, null);
                }
                
                //

                if (key == Key.A && window.paletteAnimate.IsChecked != null)
                {
                    window.paletteAnimate.IsChecked = !window.paletteAnimate.IsChecked;
                    return;
                }

                if (key == Key.D1)
                {
                    window.palettePreview.SelectedIndex = 0;
                    window.paletteEditor.UpdateDictionary(null, null);
                    return;
                }
                if (key == Key.D2)
                {
                    window.palettePreview.SelectedIndex = 1;
                    window.paletteEditor.UpdateDictionary(null, null);
                    return;
                }
                if (key == Key.D3)
                {
                    window.palettePreview.SelectedIndex = 2;
                    window.paletteEditor.UpdateDictionary(null, null);
                    return;
                }

                if (key == Key.OemComma)
                {
                    window.paletteEditor.FrameBack(null, null);
                    return;
                }
                if (key == Key.OemPeriod)
                {
                    window.paletteEditor.FrameForward(null, null);
                    return;
                }
            }
        }
    }
}
