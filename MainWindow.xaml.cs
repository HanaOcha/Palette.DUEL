using ColorPicker;
using ColorPicker.Models;
using Microsoft.Win32;
using System.IO;
using System.Numerics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows.Shapes;

namespace palette.duel
{
    public partial class MainWindow : Window
    {
        public required CharacterSelect charSelect;
        public required PaletteEditor paletteEditor;
        public required PaletteHover paletteHover;

        public DispatcherTimer watch;
        public MainWindow()
        {
            InitializeComponent();
            this.ResizeMode = ResizeMode.CanMinimize;

            Data.ReadPaletteCounts();
            Data.ReadCharacters();

            this.CharacterSelectUI();
            this.PaletteEditorUI();
            this.PaletteHoverSetup();

            this.watch = new DispatcherTimer(
                new TimeSpan(0, 0, 0, 0, (int)(1000f / 14f)), 
                DispatcherPriority.Background,
                this.paletteEditor.Animate, Dispatcher.CurrentDispatcher);
            this.watch.IsEnabled = false;

            this.paletteEditor.Hide();
        }
        public void CharacterSelectUI()
        {
            charSelect = new CharacterSelect(this);
            charSelect.UpdateOutfit();
            this.selectOutfitBack.Click += charSelect.OutfitBack;
            this.selectOutfitForward.Click += charSelect.OutfitForward;
            this.finalizePaint.Click += charSelect.FinalizePaint;
            charSelect.Show();
        }
        public void PaletteEditorUI()
        {
            this.paletteEditor = new PaletteEditor(this);
            this.paletteFrameBack.Click += this.paletteEditor.FrameBack;
            this.paletteFrameForward.Click += this.paletteEditor.FrameForward;
            this.paletteFrameBackTen.Click += this.paletteEditor.FrameBackT;
            this.paletteFrameForwardTen.Click += this.paletteEditor.FrameForwardT;
            this.palettePreview.DropDownClosed += this.paletteEditor.UpdateDictionary;
            this.focusOpacityToggle.Click += this.paletteEditor.UpdateDictionary;
            this.setIdleFrame.Click += this.paletteEditor.SetIdleFrame;
            this.findNextFrame.Click += this.paletteEditor.NextFrameWithFocus;
            this.importPalette.Click += this.paletteEditor.Import;
            this.exportPalette.Click += this.paletteEditor.Export;
            this.importDropper.Click += this.paletteEditor.AddReference;

            //this.colorPickRed.ValueChanged += this.paletteEditor.UpdateColorWithRGB;
            //this.colorPickGreen.ValueChanged += this.paletteEditor.UpdateColorWithRGB;
            //this.colorPickBlue.ValueChanged += this.paletteEditor.UpdateColorWithRGB;
            //this.colorPickHue.ValueChanged += this.paletteEditor.UpdateColorWithHue;
            //this.colorPickLight.ValueChanged += this.paletteEditor.UpdateColorWithValue;
            this.paletteColorWheel.ColorChanged += this.paletteEditor.ColorWithWheel;
            this.paletteColorSlider.ColorChanged += this.paletteEditor.ColorWithSlider;
            this.paletteColorHex.ColorChanged += this.paletteEditor.ColorWithHex;
            this.focusOpacity.ValueChanged += this.paletteEditor.UpdateDictionary;
            this.resetPaletteEditor.Click += this.paletteEditor.Reset;

            for (int i = 0; i < 25; i++)
            {
                this.paletteGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }
        }
        public void PaletteHoverSetup()
        {
            this.paletteHover = new PaletteHover(this);
            this.chrPaletteDisplay.MouseLeftButtonDown += this.paletteHover.ColorPoint;
        }
        private void ReadShortcuts(object sender, KeyEventArgs e)
        {
            TextBox? anyTextBox = Keyboard.FocusedElement as TextBox;
            if (anyTextBox != null)
            {
                if (e.Key == Key.Return)
                {
                    Keyboard.Focus(this.keyCatcher);
                    // for some reason if a button isnt focused on return press on textbox shortcuts break
                }
                return;
            }

            Shortcuts.ReadKey(this, e.Key, Keyboard.Modifiers);
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
            using (FileStream stream = File.OpenRead(data.splash))
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = stream; bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                this.Background = new ImageBrush() { ImageSource = bitmap };
            }

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
        //public BitmapPalette? basePaletteKeys;
        public List<Color> baseColors = new List<Color>();
        public WriteableBitmap? editBit;
        public List<Color> defaultColors = new List<Color>();

        public List<PaletteButton> paletteButtons = new List<PaletteButton>();
        public PaletteButton? selectedPaletteBtn = null;

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

        public byte[] exportPixels = new byte[256];

        //public Color currentColor = Color.FromRgb(255, 255, 255);

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
        public void Reset(object? s = null, EventArgs? e = null)
        {
            this.paletteButtons.Clear();
            this.pixpoints.Clear();
            this.paletteMatch.Clear();
            this.defaultColors.Clear();

            this.SetupPixels();
            this.window.chrPaletteDisplay.Source = this.editBit;

            this.frame = outfit.spriteData.preview;
            this.UpdatePalette((int)frame.X, (int)frame.Y);
            this.SetPaletteFocus(null);
            this.SetSelectorColor(Color.FromRgb(255, 255, 255));

            this.window.focusOpacity.Value = 25;
        }
        public void SetupPixels()
        {
            using (FileStream stream = File.OpenRead(outfit.sheet))
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                this.baseBit = new FormatConvertedBitmap(bitmap, PixelFormats.Bgra32, null, 0);
            }
            this.editBit = new WriteableBitmap(this.baseBit);

            this.stride = (this.baseBit.PixelWidth * this.baseBit.Format.BitsPerPixel + 7) / 8;
            this.allPixels = new byte[this.stride * this.baseBit.PixelHeight];
            this.baseBit.CopyPixels(allPixels, this.stride, 0);

            for (int x = 0; x < this.baseBit.Width; x++)
            {
                for (int y = 0; y < this.baseBit.Height; y++)
                {
                    int j = (y * (int)this.baseBit.PixelWidth + x) * 4;
                    if (this.allPixels[j + 3] > 0)
                    {
                        this.pixpoints.Add(
                            new Pixpoint(x, y, 
                            Color.FromArgb(
                                this.allPixels[j+3],
                                this.allPixels[j+2], 
                                this.allPixels[j+1], 
                                this.allPixels[j]))
                            );
                    }
                }
            }

            using (FileStream stream = File.OpenRead(outfit.palette))
            {
                BitmapImage imageMap = new BitmapImage();
                imageMap.BeginInit();
                imageMap.StreamSource = stream; imageMap.CacheOption = BitmapCacheOption.OnLoad;
                imageMap.EndInit();

                BitmapPalette basePalette = new BitmapPalette(imageMap, 100);

                FormatConvertedBitmap bitmap = new FormatConvertedBitmap(imageMap, PixelFormats.Bgra32, null, 0);
                
                int s = (100 * bitmap.Format.BitsPerPixel + 7) / 8;
                byte[] pixels = new byte[s];
                bitmap.CopyPixels(pixels, s, 0);

                for (int i = 0; i < bitmap.PixelWidth; i++)
                {
                    int j = i * 4;
                    Color color = Color.FromArgb(pixels[j + 3], pixels[j + 2], pixels[j + 1], pixels[j]);
                    if (color.A > 0)
                    {
                        this.baseColors.Add(color);
                    }
                }
            }
            using (FileStream stream = File.OpenRead(outfit.normal))
            {
                BitmapImage imageMap = new BitmapImage();
                imageMap.BeginInit();
                imageMap.StreamSource = stream; imageMap.CacheOption = BitmapCacheOption.OnLoad;
                imageMap.EndInit();

                BitmapPalette defaultPalette = new BitmapPalette(imageMap, 100);
                FormatConvertedBitmap bitmap = new FormatConvertedBitmap(imageMap, PixelFormats.Bgra32, defaultPalette, 0);
                
                int s = (100 * bitmap.Format.BitsPerPixel + 7) / 8;
                byte[] pixels = new byte[s];
                bitmap.CopyPixels(pixels, s, 0);

                for (int i = 0; i < this.baseColors.Count; i++)
                {
                    int j = i * 4;
                    Color color = Color.FromArgb(pixels[j + 3], pixels[j + 2], pixels[j + 1], pixels[j]);
                    this.defaultColors.Add(color);
                }
            }

            foreach (Color c in this.baseColors)
            {
                if (!(c.R == 205 && c.G == 205 && c.B == 205))
                {
                    this.paletteMatch.Add(c, c);
                    this.CreatePaletteButton(c);
                }
            }

            int exportStride = (100 * this.editBit.Format.BitsPerPixel + 7) / 8;
            this.exportPixels = new byte[exportStride * 1];
        }
        public PaletteButton CreatePaletteButton(Color key)
        {
            PaletteButton button = new PaletteButton(this.window, this, key);

            int x = paletteButtons.Count % 25;
            int y = (int)Math.Floor(paletteButtons.Count / 25f);

            this.window.paletteGrid.Children.Add(button);
            Grid.SetRow(button, y);
            Grid.SetColumn(button, x);
            button.CreateImageBack();
            this.paletteButtons.Add(button);

            return button;
        }

        public void UpdateDictionary(object? sender, EventArgs? e)
        {
            string value = this.window.palettePreview.Text;

            switch (value)
            {
                case "Preview":
                    foreach (PaletteButton button in this.paletteButtons)
                    {
                        this.paletteMatch[button.key] = button.value;
                    }
                    break;
                case "Default":
                    for (int i = 0; i < this.baseColors.Count; i++)
                    {
                        Color bColor = this.baseColors[i];
                        Color dColor = this.defaultColors[i];

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

            if (this.selectedPaletteBtn != null && this.window.focusOpacityToggle.IsChecked == true)
            {
                foreach (Color key in this.paletteMatch.Keys)
                {
                    Color color = this.paletteMatch[key];
                    if (this.selectedPaletteBtn.key != key)
                    {
                        color.A = (byte)(int)(this.window.focusOpacity.Value * 255f / 100f);
                    }
                    this.paletteMatch[key] = color;
                }
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
        public void SetIdleFrame(object? sender, EventArgs? e)
        {
            this.frame = this.outfit.spriteData.preview;
            this.UpdatePalette((int)frame.X, (int)frame.Y);
        }

        public void SetPaletteFocus(PaletteButton? button)
        {
            foreach (PaletteButton _button in this.paletteButtons)
            {
                _button.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            }

            selectedPaletteBtn = null;
            if (button != null)
            {
                button.BorderBrush = new SolidColorBrush(Color.FromRgb(238, 50, 98));
                selectedPaletteBtn = button;
                this.SetSelectorColor(button.value);
            }

            UpdateDictionary(null, null);
        }
        public void NextFrameWithFocus(object? sender, EventArgs? e)
        {
            if (selectedPaletteBtn != null)
            {
                Color key = selectedPaletteBtn.key;
                List<Vector2> frames = new List<Vector2>();
                List<Vector2> after = new List<Vector2>();

                foreach (Pixpoint point in this.pixpoints) 
                {
                    if (point.color == key)
                    {
                        int frameX = (int)Math.Floor((float)point.x / outfit.spriteData.size.X);
                        int frameY = (int)Math.Floor((float)point.y / outfit.spriteData.size.Y);
                        Vector2 f = new Vector2(frameX, frameY);
                        frames.Add(f);

                        if (f.Y > frame.Y || (f.Y == frame.Y && f.X > frame.X))
                        {
                            after.Add(f);
                        }
                    }
                }

                after.Sort((x, y) => (x.X == y.X && x.Y == y.Y) ? 0 :
                    (x.Y < y.Y || (x.Y == y.Y && x.X < y.X)) ? -1 : 1
                );
                frames.Sort((x, y) => (x.X == y.X && x.Y == y.Y) ? 0 :
                    (x.Y < y.Y || (x.Y == y.Y && x.X < y.X)) ? -1 : 1
                );

                if (after.Count > 0)
                {
                    Vector2 f = after[0];
                    this.frame = f;
                    this.UpdatePalette((int)f.X, (int)f.Y);
                    return;
                }

                if (frames.Count > 0)
                {
                    Vector2 f = frames[0];
                    this.frame = f;
                    this.UpdatePalette((int)f.X, (int)f.Y);
                }
            }
        }

        //public void UpdateColorDisplay(object? s = null, EventArgs? e = null)
        //{
        //    this.window.paletteSelectedColor.Fill = new SolidColorBrush(this.currentColor);

        //    this.window.colorPickRedValue.Text = this.currentColor.R.ToString();
        //    this.window.colorPickGreenValue.Text = this.currentColor.G.ToString();
        //    this.window.colorPickBlueValue.Text = this.currentColor.B.ToString();

        //    this.window.colorPickLightValue.Text = this.window.colorPickLight.Value.ToString();
        //}

        //public void UpdateColorWithRGB(object? sender, EventArgs? e)
        //{
        //    System.Drawing.Color hsv = System.Drawing.Color.FromArgb(
        //        (int)this.window.colorPickRed.Value,
        //        (int)this.window.colorPickGreen.Value,
        //        (int)this.window.colorPickBlue.Value
        //        );

        //    this.window.colorPickHue.Value = (int)hsv.GetHue();
        //    this.window.colorPickLight.Value = (int)(Math.Max(hsv.R, Math.Max(hsv.G, hsv.B))/255f * 100f);

        //    this.currentColor = Color.FromRgb(hsv.R, hsv.G, hsv.B);
        //    this.UpdateColorValue(this.currentColor);
        //}
        //public void UpdateColorWithHue(object? sender, EventArgs? e)
        //{


        //    this.UpdateColorValue(this.currentColor);
        //}
        //public void UpdateColorWithValue(object? sender, EventArgs? e)
        //{
        //    Color c = this.currentColor;
        //    int highest = Math.Max((byte)1, Math.Max(c.R, Math.Max(c.G, c.B)));
        //    float r = c.R / (float)highest;
        //    float g = c.G / (float)highest;
        //    float b = c.B / (float)highest;

        //    int _new = (int)(this.window.colorPickLight.Value / 100f * 255f);
        //    this.currentColor = Color.FromRgb((byte)(int)(r * _new), (byte)(int)(g * _new), (byte)(int)(b * _new));

        //    this.window.colorPickRed.Value = this.currentColor.R;
        //    this.window.colorPickGreen.Value = this.currentColor.G;
        //    this.window.colorPickBlue.Value = this.currentColor.B;

        //    this.UpdateColorValue(this.currentColor);
        //}
        public void SetSelectorColor(Color color)
        {
            //this.window.colorPickRed.Value = color.R;
            //this.window.colorPickGreen.Value = color.G;
            //this.window.colorPickBlue.Value = color.B;
            //this.currentColor = color;

            //this.UpdateColorWithRGB(null, null);

            this.window.paletteColorWheel.SelectedColor = color;
            this.window.paletteColorSlider.SelectedColor = color;
            this.window.paletteColorHex.SelectedColor = color;
        }
        public void UpdateColorValue(Color color)
        {
            if (this.selectedPaletteBtn != null)
            {
                this.selectedPaletteBtn.Set(color);
                this.UpdateDictionary(null, null);
            }
        }
        public void ColorWithWheel(object? sender, EventArgs? e)
        {
            Color color = this.window.paletteColorWheel.SelectedColor;
            this.window.paletteColorSlider.SelectedColor = color;
            this.window.paletteColorHex.SelectedColor = color;
            this.UpdateColorValue(color);
        }
        public void ColorWithSlider(object? sender, EventArgs? e)
        {
            Color color = this.window.paletteColorSlider.SelectedColor;
            this.window.paletteColorWheel.SelectedColor = color;
            this.window.paletteColorHex.SelectedColor = color;
            this.UpdateColorValue(color);
        }
        public void ColorWithHex(object? sender, EventArgs? e)
        {
            Color color = this.window.paletteColorHex.SelectedColor;
            this.window.paletteColorSlider.SelectedColor = color;
            this.window.paletteColorWheel.SelectedColor = color;
            this.UpdateColorValue(color);
        }

        public void Import(object? sender, EventArgs? e)
        {
            this.SetPaletteFocus(null);

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.InitialDirectory = Data.PATH.I;
            dialog.Filter = ".PNG Format (*.png)|*.png";
            dialog.Title = "Select a palette file to import.";
            dialog.RestoreDirectory = true;

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                using (FileStream stream = File.OpenRead(dialog.FileName))
                {
                    BitmapImage imageMap = new BitmapImage();
                    imageMap.BeginInit();
                    imageMap.StreamSource = stream; imageMap.CacheOption = BitmapCacheOption.OnLoad;
                    imageMap.EndInit();
                    BitmapPalette paletteMap = new BitmapPalette(imageMap, 100);

                    BitmapSource bitmap = new FormatConvertedBitmap(imageMap, PixelFormats.Bgra32, paletteMap, 0);

                    int stride = (bitmap.PixelWidth * bitmap.Format.BitsPerPixel + 7) / 8;
                    byte[] pixels = new byte[stride * bitmap.PixelHeight];
                    bitmap.CopyPixels(pixels, stride, 0);

                    for (int i = 0; i < this.paletteButtons.Count; i++)
                    {
                        int j = i * 4;
                        if (pixels.Length > j + 3)
                        {
                            Color color = Color.FromArgb(pixels[j + 3], pixels[j + 2], pixels[j + 1], pixels[j]);
                            this.paletteButtons[i].Set(color);
                        }
                    }
                }

                UpdateDictionary(sender, e);
            }
        }
        public void Export(object? sender, EventArgs? e)
        {
            this.SetPaletteFocus(null);

            List<Color> colors = new List<Color>(this.paletteMatch.Values);
            colors.Add(Color.FromArgb(0, 0, 0, 0));
            BitmapPalette palette = new BitmapPalette(colors);
            WriteableBitmap bitmap = new WriteableBitmap(
                100, 1, this.editBit.DpiX, this.editBit.DpiY, PixelFormats.Bgra32, palette
                );
            // BGRA

            for (int i = 0; i < this.baseColors.Count; i++)
            {
                Color key = this.baseColors[i];
                int j = i * 4;

                if (this.paletteMatch.ContainsKey(key) && this.exportPixels.Length > j)
                {
                    Color c = this.paletteMatch[key];
                    
                    this.exportPixels[j] = c.B;
                    this.exportPixels[j + 1] = c.G;
                    this.exportPixels[j + 2] = c.R;
                    this.exportPixels[j + 3] = c.A;
                }
            }

            int exportStride = (100 * this.editBit.Format.BitsPerPixel + 7) / 8;
            bitmap.WritePixels(new Int32Rect(0, 0, 100, 1), this.exportPixels, exportStride, 0);
            
            SaveFileDialog save = new SaveFileDialog();
            save.InitialDirectory = Data.PATH.I;
            save.Filter = ".PNG Format (*.png)|*.png";
            save.Title = "Save the current palette.";
            save.RestoreDirectory = true;
            save.AddToRecent = true;
            save.AddExtension = true;
            save.FileName = character.name + "_" + outfit.name + ".png";

            if (save.ShowDialog() == true)
            {
                using (FileStream stream = new FileStream(save.FileName, FileMode.OpenOrCreate))
                {
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmap));
                    encoder.Save(stream);
                }
            }
        }
        public void AddReference(object? s, EventArgs? e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.InitialDirectory = Data.PATH.I;
            dialog.Filter = "Image|*.png;*.jpg;*.jpeg";
            dialog.Title = "Select an image file to import.";
            dialog.RestoreDirectory = true;

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                Window dropper = new DropperWindow(this.window, dialog.FileName);
                dropper.Owner = this.window;
                dropper.Show();

                this.window.Closed += new EventHandler((x, y) => dropper.Close());
            }
        }
    }
    public class PaletteButton : Button
    {
        public PaletteEditor menu;
        public Color key;
        public Color value;

        public Image back;

        public int size = 24;
        public PaletteButton(MainWindow window, PaletteEditor menu, Color key)
        {
            this.menu = menu;
            this.key = key;
            this.Style = (Style)this.menu.window.Resources["NoBGChange"];

            this.RenderSize = new Size(this.size, this.size);
            this.Width = this.size; this.Height = this.size;
            this.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            this.TabIndex = menu.paletteButtons.Count;
            this.BorderThickness = new Thickness(2);

            this.back = new Image();

            this.Set(key);
            this.CreateContext();
            this.Click += this.PaletteFocus;
        }
        public void CreateContext()
        {
            ContextMenu context = new ContextMenu();
            context.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            context.Background = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            context.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            context.BorderThickness = new Thickness(2, 2, 2, 2);
            context.Closed += this.CloseContext;

            MenuItem toBase = new MenuItem(); context.Items.Add(toBase);
            toBase.Header = "set to base color";
            toBase.Click += new RoutedEventHandler((x, y) => this.Set(this.key));
            toBase.Icon = new Rectangle(); ((Rectangle)toBase.Icon).Fill = new SolidColorBrush(this.key);

            int colorIndex = this.menu.paletteButtons.Count;
            if (this.menu.defaultColors.Count > colorIndex)
            {
                MenuItem toDefault = new MenuItem(); context.Items.Add(toDefault);
                toDefault.Header = "set to default color";
                Color defaultColor = this.menu.defaultColors[colorIndex];
                toDefault.Click += new RoutedEventHandler((x, y) => this.Set(defaultColor));
                toDefault.Icon = new Rectangle(); ((Rectangle)toDefault.Icon).Fill = new SolidColorBrush(defaultColor);
            }
            
            foreach (MenuItem item in context.Items)
            {
                item.FontSize = 14;
            }

            this.ContextMenu = context;
        }
        public void CreateImageBack()
        {
            using (FileStream stream = File.OpenRead(Data.PATH.A + "icons\\paletteBack.png"))
            {
                BitmapImage imageMap = new BitmapImage();
                imageMap.BeginInit();
                imageMap.StreamSource = stream; imageMap.CacheOption = BitmapCacheOption.OnLoad;
                imageMap.EndInit();

                this.back.Source = imageMap;
            }
            this.back.Width = this.size; this.back.Height = this.size;
            this.menu.window.paletteGrid.Children.Add(this.back);
            Grid.SetRow(this.back, Grid.GetRow(this));
            Grid.SetColumn(this.back, Grid.GetColumn(this));
            Grid.SetZIndex(this.back, Grid.GetZIndex(this) - 1);
        }
        public void PaletteFocus(object? sender, EventArgs? e)
        {
            if (menu.selectedPaletteBtn == this)
            {
                this.menu.SetPaletteFocus(null);
                return;
            }
            this.menu.SetPaletteFocus(this);
        }
        public void Set(Color color)
        {
            this.value = color;
            this.Background = new SolidColorBrush(color);
        }
        public void CloseContext(object? sender, EventArgs e)
        {
            if (this.menu.selectedPaletteBtn == this)
            {
                this.menu.SetSelectorColor(value);
            }
            this.menu.UpdateDictionary(null, null);
        }
    }

    public class PaletteHover
    {
        MainWindow window;
        PaletteEditor editor;
        public PaletteHover(MainWindow window)
        {
            this.window = window;
            this.editor = window.paletteEditor;
        }

        public bool GetPixel(out PaletteEditor.Pixpoint pixpoint)
        {
            Point mouse = Mouse.GetPosition(this.window.chrPaletteDisplay);

            //Vector2 localPixel = new Vector2(
            //    MathF.Ceiling((float)mouse.X / 3.5f), MathF.Ceiling((float)mouse.Y / 3.5f)
            //    );
            //Vector2 framePos = this.editor.frame * this.editor.outfit.spriteData.size;

            // Mouse already accounts size and offset

            int x = (int)MathF.Round((float)mouse.X);
            int y = (int)MathF.Round((float)mouse.Y);

            foreach (PaletteEditor.Pixpoint point in editor.pixpoints)
            {
                if (point.x == x && point.y == y)
                {
                    pixpoint = point;
                    return true;
                }
            }

            pixpoint = new PaletteEditor.Pixpoint(0, 0, new Color());
            return false;
        }
        public void ColorPoint(object? o, EventArgs? e)
        {
            if (GetPixel(out PaletteEditor.Pixpoint point))
            {
                foreach (PaletteButton btn in this.editor.paletteButtons)
                {
                    if (btn.key == point.color)
                    {
                        btn.PaletteFocus(null, null);   
                        break;
                    }
                }
            }
        }
    }
}