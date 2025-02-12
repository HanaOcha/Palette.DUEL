using System.IO;
using System.Numerics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace palette.duel
{
    public partial class MainWindow : Window
    {
        public CharacterSelect charSelect;
        public PaletteEditor paletteEditor;

        public DispatcherTimer watch;
        public MainWindow()
        {
            InitializeComponent();
            this.ResizeMode = ResizeMode.CanMinimize;

            Data.ReadPaletteCounts();
            Data.ReadCharacters();

            //

            charSelect = new CharacterSelect(this);
            charSelect.UpdateOutfit();
            this.selectOutfitBack.Click += charSelect.OutfitBack;
            this.selectOutfitForward.Click += charSelect.OutfitForward;
            this.finalizePaint.Click += charSelect.FinalizePaint;
            charSelect.Show();

            //

            this.paletteEditor = new PaletteEditor(this);
            this.paletteFrameBack.Click += this.paletteEditor.FrameBack;
            this.paletteFrameForward.Click += this.paletteEditor.FrameForward;
            this.paletteFrameBackTen.Click += this.paletteEditor.FrameBackT;
            this.paletteFrameForwardTen.Click += this.paletteEditor.FrameForwardT;
            this.palettePreview.DropDownClosed += this.paletteEditor.UpdateDictionary;

            this.watch = new DispatcherTimer(new TimeSpan(0, 0, 0, 0, (int)(1000f / 14f)), DispatcherPriority.Background,
                this.paletteEditor.Animate, Dispatcher.CurrentDispatcher);
            this.watch.IsEnabled = false;

            this.paletteEditor.Hide();
        }
    }
    public class DuelistButton : Button
    {
        public MainWindow? window;
        public string test = "";

        public DuelistButton() : base()
        {
            this.Init();
        }
        public DuelistButton(MainWindow window) : base()
        {
            this.window = window;
            this.Style = (Style)this.window.Resources["NoBGChange"];
            this.Init();
        }
        private void Init()
        {
            this.BorderThickness = new Thickness(2);

            this.MouseEnter += OnPointerEnter;
            this.MouseLeave += OnPointerExit;
            this.Click += OnClick;
            this.OnPointerExit(null, null);
        }
        public virtual void OnPointerEnter(object? sender, EventArgs? e)
        {
            this.BorderBrush = new SolidColorBrush(Color.FromRgb(238, 50, 98));
        }
        public virtual void OnPointerExit(object? sender, EventArgs? e)
        {
            this.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 255, 255));
        }
        public virtual void OnClick(object? s, EventArgs? e)
        {

        }
    }

    public class CharacterSelect
    {
        public MainWindow window;
        public List<CharacterButton> chrButtons = new List<CharacterButton>();

        public Vector2 btnStart = new Vector2(15, 30);
        public Vector2 btnSpacing = new Vector2(5, 5);

        public Character currentCharacter;
        public int _outfit = 0;
        public CharacterSelect(MainWindow window)
        {
            this.window = window;

            foreach (Character character in Data.characters)
            {
                CreateCharacter(character);
            }

            currentCharacter = Data.characters[0];
            foreach (Character chr in Data.characters)
            {
                if (chr.name == "Saffron")
                {
                    currentCharacter = chr;
                    break;
                }
            }
        }
        public CharacterButton CreateCharacter(Character character)
        {
            CharacterButton button = new CharacterButton(this.window, this, character);
            button.Height = (this.window.chrGrid.Height - 40) / 4f; button.Width = (this.window.chrGrid.Width - 50) / 5f ;

            int x = chrButtons.Count % 5;
            int y = (int)Math.Floor(chrButtons.Count / 5f);

            this.window.chrGrid.Children.Add(button);
            Grid.SetRow(button, y);
            Grid.SetColumn(button, x);
            this.chrButtons.Add(button);
            return button;
        }

        public void OutfitForward(object? sender, EventArgs? e)
        {
            if (_outfit + 1 < currentCharacter.outfits.Count)
            {
                _outfit++;
            }
            else
            {
                _outfit = 0;
            }
            UpdateOutfit();
        }
        public void OutfitBack(object? sender, EventArgs? e)
        {
            if (_outfit + 1 > 1)
            {
                _outfit--;
            }
            else
            {
                _outfit = currentCharacter.outfits.Count - 1;
            }

            UpdateOutfit();
        }
        public void UpdateOutfit()
        {
            Outfit outfit = this.currentCharacter.outfits[_outfit];
            this.window.selectedOutfit.Text = currentCharacter.outfits[_outfit].name;

            BitmapImage bitmap = new BitmapImage();
            using (FileStream stream = File.OpenRead(outfit.sheet))
            {
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
            }
            this.window.chrSelectDisplay.Source = bitmap;

            this.window.chrSelectDisplay.Clip = new RectangleGeometry(
                new Rect(
                    outfit.spriteData.size.X * outfit.spriteData.preview.X,
                    outfit.spriteData.size.Y * outfit.spriteData.preview.Y, 
                    outfit.spriteData.size.X, 
                    outfit.spriteData.size.Y
                    )
                );

            double left_main = this.window.Width - (140 - (140 - outfit.spriteData.size.X) / 2) * 3.5f;
            double up_main = (160 - outfit.spriteData.size.Y) / 2 * 3.5f;
            double left_boost = outfit.spriteData.size.X * outfit.spriteData.preview.X * 3.5f;
            double up_boost = outfit.spriteData.size.Y * outfit.spriteData.preview.Y * 3.5f;

            Canvas.SetLeft(this.window.chrSelectDisplay, left_main - left_boost);
            Canvas.SetTop(this.window.chrSelectDisplay, up_main - up_boost);
        }

        public void Show()
        {
            this.window.characterSelectCanvas.Visibility = Visibility.Visible;
            this.window.characterSelectCanvas.IsEnabled = true;
        }
        public void Hide()
        {
            this.window.characterSelectCanvas.Visibility = Visibility.Hidden;
            this.window.characterSelectCanvas.IsEnabled = false;
        }
        public void FinalizePaint(object? sender, EventArgs? e)
        {
            Hide();
            this.window.paletteEditor.character = this.currentCharacter;
            this.window.paletteEditor.outfit = this.currentCharacter.outfits[_outfit];

            this.window.paletteEditor.Reset();
            this.window.paletteEditor.Show();
        }
    }
    public class CharacterButton : DuelistButton
    {
        public CharacterSelect menu;
        public Character data;

        public int wSize = 120;
        public CharacterButton(MainWindow window, CharacterSelect menu, Character data) : base(window)
        {
            this.menu = menu;
            this.data = data;

            this.RenderSize = new Size(this.wSize, (int)(this.wSize * 332f / 512f));
            this.Background = new ImageBrush() { ImageSource = new BitmapImage(new Uri(data.splash)) };

            this.TabIndex = menu.chrButtons.Count;
        }
        public override void OnClick(object? sender, EventArgs? e)
        {
            this.menu.currentCharacter = data;
            this.window.selectedChar.Text = this.data.name;
            this.window.selectedOutfit.Text = this.data.outfits[0].name;
            this.menu._outfit = 0;
            this.menu.UpdateOutfit();

            base.OnClick(sender, e);
        }
    }

    public class PaletteEditor
    {
        public MainWindow window;

        public Character character;
        public Outfit outfit;
        public Vector2 frame = new Vector2();

        public BitmapSource? baseBit;
        public WriteableBitmap? editBit;

        public class Pixpoint
        {
            public int x, y;
            public Color color;
            public Pixpoint(int x, int y, Color color)
            {
                this.x = x;
                this.y = y;
                this.color = color;
            }
        }

        public int stride = 0;
        public byte[] allPixels = new byte[256];
        public List<Pixpoint> pixpoints = new List<Pixpoint>();
        public Dictionary<Color, Color> paletteMatch = new Dictionary<Color, Color>();

        public PaletteEditor(MainWindow window)
        {
            this.window = window;
            this.character = Data.characters[0];
            this.outfit = this.character.outfits[0];
        }
        public void Hide()
        {
            this.window.paletteEditCanvas.Visibility = Visibility.Hidden;
            this.window.paletteEditCanvas.IsEnabled = false;
            this.window.watch.IsEnabled = false;
        }
        public void Show()
        {
            this.window.paletteEditCanvas.Visibility = Visibility.Visible;
            this.window.paletteEditCanvas.IsEnabled = true;
            this.window.watch.IsEnabled = true;
        }
        public void Reset()
        {
            this.SetupPixels();
            this.window.chrPaletteDisplay.Source = this.editBit;

            frame = new Vector2();
            UpdatePalette((int)frame.X, (int)frame.Y);
        }
        public void SetupPixels()
        {
            this.baseBit = new FormatConvertedBitmap(
                    new BitmapImage(new Uri(outfit.sheet)), PixelFormats.Bgra32, null, 0
                );
            this.editBit = new WriteableBitmap(this.baseBit);

            this.stride = (this.baseBit.PixelWidth * this.baseBit.Format.BitsPerPixel + 7) / 8;
            allPixels = new byte[this.stride * this.baseBit.PixelHeight];
            this.baseBit.CopyPixels(allPixels, this.stride, 0);

            for (int x = 0; x < this.baseBit.Width; x++)
            {
                for (int y = 0; y < this.baseBit.Height; y++)
                {
                    int j = (y * (int)this.baseBit.PixelWidth + x) * 4;
                    if (this.allPixels[j + 3] == 255)
                    {
                        this.pixpoints.Add(
                            new Pixpoint(x, y, 
                            Color.FromRgb(
                                this.allPixels[j+2], 
                                this.allPixels[j+1], 
                                this.allPixels[j]))
                            );
                    }
                }
            }
            Console.WriteLine(this.pixpoints.Count);

            BitmapPalette basePalette = new BitmapPalette(new BitmapImage(new Uri(outfit.palette)), 100);
            foreach (Color c in basePalette.Colors)
            {
                if (!(c.R == 205 && c.G == 205 && c.B == 205))
                {
                    paletteMatch.Add(c, c);
                }
            }
        }

        public void UpdateDictionary(object? sender, EventArgs? e)
        {
            string value = this.window.palettePreview.Text;
            Console.WriteLine(value);
            switch (value)
            {
                case "Preview":
                    break;
                case "Default":
                    BitmapPalette bp = new BitmapPalette(new BitmapImage(new Uri(outfit.palette)), 100);
                    BitmapPalette dp = new BitmapPalette(new BitmapImage(new Uri(outfit.normal)), 100);
                    for (int i = 0; i < bp.Colors.Count; i++)
                    {
                        Color bColor = bp.Colors[i];
                        Color dColor = dp.Colors[i];

                        if (this.paletteMatch.ContainsKey(bColor))
                        {
                            this.paletteMatch[bColor] = dColor;
                        }
                    }
                    break;

                    // base palette
                default:
                    foreach (Color c in this.paletteMatch.Keys)
                    {
                        this.paletteMatch[c] = c;
                    }
                    break;
            }

            UpdatePixels();
        }
        public void UpdatePixels()
        {
            foreach (Pixpoint point in this.pixpoints)
            {
                int i = (point.y * this.baseBit.PixelWidth + point.x) * 4;
                if (this.paletteMatch.ContainsKey(point.color))
                {
                    Color apply = this.paletteMatch[point.color];
                    //BGRA
                    this.allPixels[i] = apply.B;
                    this.allPixels[i+1] = apply.G;
                    this.allPixels[i+2] = apply.R;
                    this.allPixels[i+3] = apply.A;
                }
            }

            this.editBit.WritePixels(
                new Int32Rect(0, 0, this.editBit.PixelWidth, this.editBit.PixelHeight), 
                this.allPixels, 
                this.stride, 
                0
            );
        }
        public void UpdatePalette(int x, int y)
        {
            this.window.paletteFrameCount.Text = "Frame " + (y * outfit.spriteData.count.X + x + 1).ToString() 
                + "/" 
                + (outfit.spriteData.count.X * outfit.spriteData.count.Y);

            this.window.chrPaletteDisplay.Clip = new RectangleGeometry(
                new Rect(
                    outfit.spriteData.size.X * x,
                    outfit.spriteData.size.Y * y,
                    outfit.spriteData.size.X,
                    outfit.spriteData.size.Y
                    )
                );

            double left_main = (140 - outfit.spriteData.size.X) / 2 * 3.5f;
            double up_main = (160 - outfit.spriteData.size.Y) / 2 * 3.5f;
            double left_boost = outfit.spriteData.size.X * x * 3.5f;
            double up_boost = outfit.spriteData.size.Y * y * 3.5f;

            Canvas.SetLeft(this.window.chrPaletteDisplay, left_main - left_boost);
            Canvas.SetTop(this.window.chrPaletteDisplay, up_main - up_boost);
        }

        public void Animate(object? sender, EventArgs e)
        {
            bool? flag = this.window.paletteAnimate.IsChecked;
            if (this.window.paletteEditCanvas.IsEnabled && flag != null && (bool)flag)
            {
                this.FrameForward(sender, e);
            }
        }

        public void FrameForward(object? sender, EventArgs? e)
        {
            if (this.frame.X < this.outfit.spriteData.count.X - 1
                || this.frame.Y < this.outfit.spriteData.count.Y - 1)
            {
                this.frame.X++;

                if (this.frame.X == this.outfit.spriteData.count.X)
                {
                    this.frame.X = 0;
                    this.frame.Y++;
                }
            }
            else
            {
                this.frame.X = 0;
                this.frame.Y = 0;
            }

            this.UpdatePalette((int)frame.X, (int)frame.Y);
        }
        public void FrameBack(object? sender, EventArgs? e)
        {
            if (this.frame.X > 0 || this.frame.Y > 0)
            {
                this.frame.X--;
                if (this.frame.X == -1)
                {
                    this.frame.Y--;
                    this.frame.X = this.outfit.spriteData.count.X - 1;
                }
            }
            else
            {
                this.frame.X = this.outfit.spriteData.count.X - 1;
                this.frame.Y = this.outfit.spriteData.count.Y - 1;
            }

            this.UpdatePalette((int)frame.X, (int)frame.Y);
        }
        public void FrameForwardT(object? sender, EventArgs? e)
        {
            for (int i = 0; i < 10;  i++)
            {
                FrameForward(sender, e);
            }
        }
        public void FrameBackT(object? sender, EventArgs? e)
        {
            for (int i = 0; i < 10; i++)
            {
                FrameBack(sender, e);
            }
        }
    }
}