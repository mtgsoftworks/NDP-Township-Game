// Gerekli kütüphaneleri içe aktarıyoruz.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;             // Grafik işlemleri için gerekli
using System.IO;                  // Dosya okuma/yazma işlemleri için gerekli
using System.Linq;                // LINQ sorguları için gerekli
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;       // Windows Form uygulamaları için gerekli

namespace NDP_Township_GAME
{
    // Form2 sınıfımız, Windows Forms'un Form sınıfından türetiliyor
    public partial class Form2 : Form
    {
        // Sabit değerleri tanımlıyoruz
        private const int GRID_SIZE = 8;          // Oyun alanının satır ve sütun sayısı
        private const int TILE_SIZE = 60;         // Her bir kutucuğun piksel cinsinden boyutu
        private const int INITIAL_TIME = 600;     // Başlangıçtaki toplam süre (saniye cinsinden)
        private const int MATCH_SCORE = 10;       // Eşleşme başına verilecek puan

        private const string HIGH_SCORES_FILE = "highscores.txt";  // En yüksek puanların kaydedileceği dosya
        private List<KeyValuePair<string, int>> highScores = new List<KeyValuePair<string, int>>(); // En yüksek puanları tutan liste

        private int score = 0;           // Oyuncunun güncel puanı
        private int timeLeft = INITIAL_TIME;  // Kalan süre
        private bool isPaused = false;    // Oyun duraklatıldı mı?
        private Tile selectedTile = null;  // Seçili olan kutucuk (Tile nesnesi)
        private Random random = new Random();    // Rastgele sayı üreteci
        private Panel gamePanel;
        private Tile[,] gameTiles;          // Oyun alanındaki kutucukları tutan iki boyutlu dizi
        private Color[] tileColors = {           // Kutucukların alabileceği renkler
            Color.Red,
            Color.Blue,
            Color.Green,
            Color.Yellow,
        };

        // Form2'nin yapıcı metodu, form başlatıldığında çalışır
        public Form2()
        {
            InitializeComponent();   // Tasarımcı tarafından oluşturulan bileşenleri başlatır
            SetupForm();             // Formun ayarlarını yapar
        }

        // Formun genel ayarlarını ve bileşenlerini oluşturur
        private void SetupForm()
        {
            // Formun boyutunu ayarlıyoruz
            this.Size = new Size(800, 700);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;   // Formun boyutu sabit
            this.MaximizeBox = false;                             // Büyütme butonunu devre dışı bırakıyoruz
            this.StartPosition = FormStartPosition.CenterScreen;  // Formu ekranın ortasında başlatıyoruz

            // Panel oluştur
            gamePanel = new Panel
            {
                Size = new Size(GRID_SIZE * TILE_SIZE + 2, GRID_SIZE * TILE_SIZE + 2),
                Location = new Point(50, 100),
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(gamePanel);

            // Label'ların konumlarını ayarlıyoruz
            lbl_Playername.Location = new Point(50, 30);
            lblTime.Location = new Point(250, 30);
            lblScore.Location = new Point(450, 30);

            // Pause butonu ekle
            Button pauseButton = new Button
            {
                Text = "Pause (P)",
                Size = new Size(100, 30),
                Location = new Point(650, 30)
            };
            pauseButton.Click += (s, e) => PauseGame();
            this.Controls.Add(pauseButton);

            // En Yüksek Skoru Göster butonu ekle
            Button showHighScoresButton = new Button
            {
                Text = "En Yüksek Skorlar",
                Size = new Size(100, 50),
                Location = new Point(650, 70)  // Pause butonunun hemen altında
            };
            showHighScoresButton.Click += (s, e) => ShowHighScores();
            this.Controls.Add(showHighScoresButton);

            // Timer ayarları
            gameTimer.Interval = 1000;
            gameTimer.Start();  // Zamanlayıcıyı burada başlatıyoruz

            // Grid'i başlat
            InitializeGameBoard(gamePanel);

            // Klavye kontrollerini ekle
            this.KeyPreview = true;
            this.KeyDown += Form2_KeyDown;
        }

        // Tile sınıfı ve alt sınıflarını tanımlıyoruz
        public abstract class Tile
        {
            // Kutucuğun rengi
            public Color Color { get; set; }
            // Kutucuğun görselini temsil eden PictureBox
            public PictureBox PictureBox { get; set; }
            // Kutucuğun oyun alanındaki konumu
            public Point Position { get; set; }
            // Kutucuğun yok edilip edilmediğini belirten bayrak
            public bool IsDestroyed { get; set; } = false;

            // Oyun alanındaki tüm kutucukların referansı
            protected Tile[,] gameTiles;
            // Rastgele işlemler için Random nesnesi
            protected Random random;

            // Tile sınıfının kurucusu
            public Tile(Color color, PictureBox pictureBox, Point position, Tile[,] gameTiles, Random random)
            {
                Color = color; // Rengi ayarla
                PictureBox = pictureBox; // Görseli ayarla
                Position = position; // Konumu ayarla
                this.gameTiles = gameTiles; // Oyun alanı referansını sakla
                this.random = random; // Random nesnesini sakla

                // PictureBox'ın arka plan rengini kutucuğun rengine ayarla
                PictureBox.BackColor = Color;
            }

            // Eşleşme gerçekleştiğinde çağrılan sanal metot
            public virtual void OnMatched()
            {
                // Eğer kutucuk zaten yok edildiyse işlem yapma
                if (IsDestroyed)
                    return;

                IsDestroyed = true; // Kutucuğu yok edildi olarak işaretle
                Destroy(); // Kutucuğu yok et
            }

            // Kutucuğu yok eden sanal metot
            public virtual void Destroy()
            {
                PictureBox.BackColor = Color.White; // Arka plan rengini beyaza ayarla
                PictureBox.Image = null; // Resmi kaldır
                Color = Color.White; // Rengi beyaza ayarla
            }

            // Kutucuğun joker olup olmadığını belirten sanal metot
            public virtual bool IsJoker()
            {
                return false; // Varsayılan olarak joker değil
            }

            // Mevcut kutucuk renklerinden rastgele bir renk seçen yardımcı metot
            protected Color GetRandomExistingColor()
            {
                // Mevcut renkleri saklamak için liste oluştur
                List<Color> existingColors = new List<Color>();
                // Oyun alanındaki tüm kutucukları dolaş
                for (int row = 0; row < gameTiles.GetLength(0); row++)
                {
                    for (int col = 0; col < gameTiles.GetLength(1); col++)
                    {
                        Color currentColor = gameTiles[row, col].Color; // Mevcut kutucuğun rengini al
                                                                        // Eğer renk beyaz değilse ve listeye eklenmemişse listeye ekle
                        if (currentColor != Color.White && !existingColors.Contains(currentColor))
                        {
                            existingColors.Add(currentColor);
                        }
                    }
                }
                // Mevcut renklerden rastgele birini döndür
                return existingColors[random.Next(existingColors.Count)];
            }
        }

        // Normal kutucuk sınıfı
        public class NormalTile : Tile
        {
            // NormalTile kurucusu
            public NormalTile(Color color, PictureBox pictureBox, Point position, Tile[,] gameTiles, Random random)
                : base(color, pictureBox, position, gameTiles, random)
            {
                // Ekstra işlem yok
            }
        }

        // Helikopter joker kutucuk sınıfı
        public class CopterTile : Tile
        {
            // CopterTile kurucusu
            public CopterTile(Color color, PictureBox pictureBox, Point position, Tile[,] gameTiles, Random random)
                : base(color, pictureBox, position, gameTiles, random)
            {
                PictureBox.Image = Properties.Resources.item_helicopter; // Helikopter resmini ayarla
                PictureBox.SizeMode = PictureBoxSizeMode.StretchImage; // Resmi kutucuğa sığdır
            }

            // Eşleşme gerçekleştiğinde çağrılan metot
            public override void OnMatched()
            {
                if (IsDestroyed)
                    return;

                IsDestroyed = true; // Kutucuğu yok edildi olarak işaretle

                UseCopter(); // Helikopter yeteneğini kullan
                Destroy(); // Kutucuğu yok et
            }

            // Helikopter yeteneğini gerçekleştiren metot
            private void UseCopter()
            {
                // Rastgele bir konum seç
                int randRow = random.Next(gameTiles.GetLength(0));
                int randCol = random.Next(gameTiles.GetLength(1));
                // Eğer seçilen konumdaki kutucuk varsa
                if (gameTiles[randRow, randCol] != null)
                {
                    gameTiles[randRow, randCol].OnMatched(); // Kutucuğu patlat
                }
            }

            // Kutucuğun joker olduğunu belirten metot
            public override bool IsJoker()
            {
                return true;
            }
        }

        // Bomba joker kutucuk sınıfı
        public class BombTile : Tile
        {
            // BombTile kurucusu
            public BombTile(Color color, PictureBox pictureBox, Point position, Tile[,] gameTiles, Random random)
                : base(color, pictureBox, position, gameTiles, random)
            {
                PictureBox.Image = Properties.Resources.item_dynamite; // Dinamit resmini ayarla
                PictureBox.SizeMode = PictureBoxSizeMode.StretchImage; // Resmi kutucuğa sığdır
            }

            // Eşleşme gerçekleştiğinde çağrılan metot
            public override void OnMatched()
            {
                // Eğer zaten yok edildiyse işlem yapma
                if (IsDestroyed)
                    return;

                IsDestroyed = true; // Kutucuğu yok edildi olarak işaretle

                UseBomb(); // Bomba yeteneğini kullan

                Destroy(); // Kutucuğu yok et
            }

            // Bomba yeteneğini gerçekleştiren metot
            private void UseBomb()
            {
                // Kutucuğun etrafındaki 3x3 alanı patlat
                for (int r = -1; r <= 1; r++)
                {
                    for (int c = -1; c <= 1; c++)
                    {
                        int newRow = Position.X + r; // Yeni satır indeksi
                        int newCol = Position.Y + c; // Yeni sütun indeksi

                        // Eğer indeksler oyun alanı sınırları içindeyse
                        if (newRow >= 0 && newRow < gameTiles.GetLength(0) && newCol >= 0 && newCol < gameTiles.GetLength(1))
                        {
                            // Kutucuk varsa ve yok edilmemişse
                            if (gameTiles[newRow, newCol] != null && !gameTiles[newRow, newCol].IsDestroyed)
                            {
                                gameTiles[newRow, newCol].OnMatched(); // Kutucuğu patlat
                            }
                        }
                    }
                }
            }

            // Kutucuğun joker olduğunu belirten metot
            public override bool IsJoker()
            {
                return true;
            }
        }

        // Gökkuşağı joker kutucuk sınıfı
        public class RainbowTile : Tile
        {
            // RainbowTile kurucusu
            public RainbowTile(Color color, PictureBox pictureBox, Point position, Tile[,] gameTiles, Random random)
                : base(color, pictureBox, position, gameTiles, random)
            {
                PictureBox.Image = Properties.Resources.item_rainbow_ball; // Gökkuşağı topu resmini ayarla
                PictureBox.SizeMode = PictureBoxSizeMode.StretchImage; // Resmi kutucuğa sığdır
            }

            // Eşleşme gerçekleştiğinde çağrılan metot
            public override void OnMatched()
            {
                if (IsDestroyed)
                    return;

                IsDestroyed = true; // Kutucuğu yok edildi olarak işaretle

                UseRainbow(); // Gökkuşağı yeteneğini kullan
                Destroy(); // Kutucuğu yok et
            }

            // Gökkuşağı yeteneğini gerçekleştiren metot
            private void UseRainbow()
            {
                // Mevcut renklerden rastgele birini seç
                Color targetColor = GetRandomExistingColor();

                // Oyun alanındaki tüm kutucukları kontrol et
                for (int row = 0; row < gameTiles.GetLength(0); row++)
                {
                    for (int col = 0; col < gameTiles.GetLength(1); col++)
                    {
                        // Eğer kutucuk varsa ve rengi hedef renge eşitse
                        if (gameTiles[row, col] != null && gameTiles[row, col].Color == targetColor)
                        {
                            gameTiles[row, col].OnMatched(); // Kutucuğu patlat
                        }
                    }
                }
            }

            // Kutucuğun joker olduğunu belirten metot
            public override bool IsJoker()
            {
                return true;
            }
        }

        // Roket joker kutucuk sınıfı
        public class RocketTile : Tile
        {
            // Roketin dikey mi yatay mı olduğunu belirten özellik
            public bool IsVertical { get; set; }

            // RocketTile kurucusu
            public RocketTile(Color color, PictureBox pictureBox, Point position, bool isVertical, Tile[,] gameTiles, Random random)
                : base(color, pictureBox, position, gameTiles, random)
            {
                IsVertical = isVertical; // Roketin yönünü ayarla
                                         // Resmi roketin yönüne göre ayarla
                PictureBox.Image = isVertical ? Properties.Resources.item_rocket_vertical : Properties.Resources.item_rocket_horizontal;
                PictureBox.SizeMode = PictureBoxSizeMode.StretchImage; // Resmi kutucuğa sığdır
            }

            // Eşleşme gerçekleştiğinde çağrılan metot
            public override void OnMatched()
            {
                if (IsDestroyed)
                    return;

                IsDestroyed = true; // Kutucuğu yok edildi olarak işaretle

                UseRocket(); // Roket yeteneğini kullan
                Destroy(); // Kutucuğu yok et
            }

            // Roket yeteneğini gerçekleştiren metot
            private void UseRocket()
            {
                if (IsVertical)
                {
                    // Dikey patlatma işlemi
                    for (int r = 0; r < gameTiles.GetLength(0); r++)
                    {
                        // Aynı sütundaki tüm kutucukları patlat
                        if (gameTiles[r, Position.Y] != null && gameTiles[r, Position.Y] != this)
                        {
                            gameTiles[r, Position.Y].OnMatched();
                        }
                    }
                }
                else
                {
                    // Yatay patlatma işlemi
                    for (int c = 0; c < gameTiles.GetLength(1); c++)
                    {
                        // Aynı satırdaki tüm kutucukları patlat
                        if (gameTiles[Position.X, c] != null && gameTiles[Position.X, c] != this)
                        {
                            gameTiles[Position.X, c].OnMatched();
                        }
                    }
                }
            }

            // Kutucuğun joker olduğunu belirten metot
            public override bool IsJoker()
            {
                return true;
            }
        }

        // Oyun alanını başlatan metot
        private void InitializeGameBoard(Panel gamePanel)
        {
            gameTiles = new Tile[GRID_SIZE, GRID_SIZE]; // Oyun alanını oluştur

            List<Point> positions = new List<Point>();
            // Oyun alanındaki tüm pozisyonları listeye ekle
            for (int row = 0; row < GRID_SIZE; row++)
            {
                for (int col = 0; col < GRID_SIZE; col++)
                {
                    positions.Add(new Point(row, col));
                }
            }

            // Jokerleri yerleştiriyoruz
            int jokerCount = 0;
            int maxJokers = 4; // Maksimum joker sayısı
            while (jokerCount < maxJokers)
            {
                if (positions.Count == 0) break; // Pozisyonlar tükendiğinde döngüden çık
                int index = random.Next(positions.Count); // Rastgele bir indeks seç
                Point pos = positions[index]; // Pozisyonu al
                positions.RemoveAt(index); // Pozisyonu listeden kaldır

                PictureBox pictureBox = CreatePictureBox(pos.X, pos.Y); // PictureBox oluştur
                gamePanel.Controls.Add(pictureBox); // PictureBox'ı panele ekle

                Tile tile;
                int jokerType = random.Next(4); // Rastgele joker tipi seç
                switch (jokerType)
                {
                    case 0:
                        // Roket joker
                        tile = new RocketTile(GetRandomColor(), pictureBox, pos, random.Next(2) == 0, gameTiles, random);
                        break;
                    case 1:
                        // Helikopter joker
                        tile = new CopterTile(GetRandomColor(), pictureBox, pos, gameTiles, random);
                        break;
                    case 2:
                        // Bomba joker
                        tile = new BombTile(GetRandomColor(), pictureBox, pos, gameTiles, random);
                        break;
                    case 3:
                        // Gökkuşağı joker
                        tile = new RainbowTile(GetRandomColor(), pictureBox, pos, gameTiles, random);
                        break;
                    default:
                        // Normal kutucuk
                        tile = new NormalTile(GetRandomColor(), pictureBox, pos, gameTiles, random);
                        break;
                }

                gameTiles[pos.X, pos.Y] = tile; // Kutucuğu oyun alanına ekle
                pictureBox.Tag = pos; // PictureBox'ın tag özelliğini konum olarak ayarla
                pictureBox.Click += Tile_Click; // Click olayını bağla
                jokerCount++; // Joker sayısını artır
            }

            // Geri kalan pozisyonları normal kutucuklarla dolduruyoruz
            foreach (Point pos in positions)
            {
                PictureBox pictureBox = CreatePictureBox(pos.X, pos.Y); // PictureBox oluştur
                gamePanel.Controls.Add(pictureBox); // PictureBox'ı panele ekle

                Color newColor;
                do
                {
                    newColor = GetRandomColor(); // Rastgele renk seç
                } while (WouldCauseMatch(pos.X, pos.Y, newColor)); // Başlangıçta eşleşme olmamasını sağla

                Tile tile = new NormalTile(newColor, pictureBox, pos, gameTiles, random); // Normal kutucuk oluştur
                gameTiles[pos.X, pos.Y] = tile; // Kutucuğu oyun alanına ekle
                pictureBox.Tag = pos; // PictureBox'ın tag özelliğini konum olarak ayarla
                pictureBox.Click += Tile_Click; // Click olayını bağla
            }
        }

        // PictureBox oluşturan yardımcı metot
        private PictureBox CreatePictureBox(int row, int col)
        {
            PictureBox pictureBox = new PictureBox
            {
                Name = $"Tile_{row}_{col}", // İsmi ayarla
                Size = new Size(TILE_SIZE - 2, TILE_SIZE - 2), // Boyutu ayarla
                Location = new Point(col * TILE_SIZE + 1, row * TILE_SIZE + 1), // Konumu ayarla
                BorderStyle = BorderStyle.FixedSingle, // Kenarlık stilini ayarla
                BackColor = Color.White // Arka plan rengini beyaz yap
            };
            return pictureBox;
        }

        // Rastgele renk seçen metot
        private Color GetRandomColor()
        {
            return tileColors[random.Next(tileColors.Length)]; // tileColors dizisinden rastgele bir renk döndür
        }

        // Başlangıçta eşleşme olup olmadığını kontrol eden metot
        private bool WouldCauseMatch(int row, int col, Color newColor)
        {
            // Yatay kontrol
            if (col >= 2)
            {
                // Sol taraftaki iki kutucuk aynı renkteyse eşleşme oluşur
                if (gameTiles[row, col - 1]?.Color == newColor && gameTiles[row, col - 2]?.Color == newColor)
                {
                    return true;
                }
            }
            // Dikey kontrol
            if (row >= 2)
            {
                // Yukarıdaki iki kutucuk aynı renkteyse eşleşme oluşur
                if (gameTiles[row - 1, col]?.Color == newColor && gameTiles[row - 2, col]?.Color == newColor)
                {
                    return true;
                }
            }
            return false; // Eşleşme yok
        }

        // Mevcut renklerden rastgele birini seçen metot
        private Color GetRandomExistingColor()
        {
            List<Color> existingColors = new List<Color>();
            for (int row = 0; row < GRID_SIZE; row++)
            {
                for (int col = 0; col < GRID_SIZE; col++)
                {
                    Color currentColor = gameTiles[row, col].Color;
                    if (currentColor != Color.White && !existingColors.Contains(currentColor))
                    {
                        existingColors.Add(currentColor);
                    }
                }
            }
            return existingColors[random.Next(existingColors.Count)];
        }

        // Kutucuğa tıklandığında tetiklenen olay
        private void Tile_Click(object sender, EventArgs e)
        {
            if (isPaused) return; // Eğer oyun duraklatıldıysa işlem yapma

            PictureBox clickedPictureBox = sender as PictureBox;
            Point pos = (Point)clickedPictureBox.Tag;
            Tile clickedTile = gameTiles[pos.X, pos.Y];

            if (selectedTile == null)
            {
                SelectTile(clickedTile); // Kutucuğu seç
            }
            else
            {
                if (AreAdjacent(selectedTile, clickedTile))
                {
                    SwapTiles(selectedTile, clickedTile); // Kutucukları değiştir

                    // Eşleşme kontrolü
                    if (!CheckAndHandleMatches())
                    {
                        // Eşleşme yoksa değişikliği geri al
                        SwapTiles(selectedTile, clickedTile);
                    }
                }
                DeselectTile(); // Seçimi kaldır
            }
        }

        // Kutucuğu seçili hale getirir
        private void SelectTile(Tile tile)
        {
            selectedTile = tile; // Seçili kutucuğu ayarla
            tile.PictureBox.BorderStyle = BorderStyle.Fixed3D; // Kenarlığı 3D yap
        }

        // Seçimi kaldırır
        private void DeselectTile()
        {
            if (selectedTile != null)
            {
                selectedTile.PictureBox.BorderStyle = BorderStyle.FixedSingle; // Kenarlığı normal yap
                selectedTile = null; // Seçili kutucuğu sıfırla
            }
        }

        // İki kutucuğun yan yana olup olmadığını kontrol eder
        private bool AreAdjacent(Tile tile1, Tile tile2)
        {
            Point pos1 = tile1.Position;
            Point pos2 = tile2.Position;

            // Eğer kutucuklar yan yanaysa pozisyonlarının toplam farkı 1 olur
            return Math.Abs(pos1.X - pos2.X) + Math.Abs(pos1.Y - pos2.Y) == 1;
        }

        // İki kutucuğun yerlerini değiştirir
        private void SwapTiles(Tile tile1, Tile tile2)
        {
            // Geçici değişkenlerle özellikleri değiştiriyoruz
            Color tempColor = tile1.Color;
            Image tempImage = tile1.PictureBox.Image;
            Point tempPosition = tile1.Position;
            object tempTag = tile1.PictureBox.Tag;

            tile1.Color = tile2.Color;
            tile1.PictureBox.BackColor = tile2.Color;
            tile1.PictureBox.Image = tile2.PictureBox.Image;
            tile1.Position = tile2.Position;
            tile1.PictureBox.Tag = tile2.PictureBox.Tag;

            tile2.Color = tempColor;
            tile2.PictureBox.BackColor = tempColor;
            tile2.PictureBox.Image = tempImage;
            tile2.Position = tempPosition;
            tile2.PictureBox.Tag = tempTag;
        }

        // Eşleşmeleri kontrol eder ve işlemleri yapar
        private bool CheckAndHandleMatches()
        {
            List<Tile> matchedTiles = new List<Tile>();

            // Yatay kontrol
            for (int row = 0; row < GRID_SIZE; row++)
            {
                for (int col = 0; col < GRID_SIZE - 2; col++)
                {
                    Color currentColor = gameTiles[row, col].Color;
                    if (currentColor == Color.White) continue;

                    if (gameTiles[row, col + 1].Color == currentColor && gameTiles[row, col + 2].Color == currentColor)
                    {
                        matchedTiles.Add(gameTiles[row, col]);
                        matchedTiles.Add(gameTiles[row, col + 1]);
                        matchedTiles.Add(gameTiles[row, col + 2]);
                    }
                }
            }

            // Dikey kontrol
            for (int col = 0; col < GRID_SIZE; col++)
            {
                for (int row = 0; row < GRID_SIZE - 2; row++)
                {
                    Color currentColor = gameTiles[row, col].Color;
                    if (currentColor == Color.White) continue;

                    if (gameTiles[row + 1, col].Color == currentColor && gameTiles[row + 2, col].Color == currentColor)
                    {
                        matchedTiles.Add(gameTiles[row, col]);
                        matchedTiles.Add(gameTiles[row + 1, col]);
                        matchedTiles.Add(gameTiles[row + 2, col]);
                    }
                }
            }

            if (matchedTiles.Any())
            {
                HandleMatches(matchedTiles.Distinct().ToList()); // Eşleşmeleri işle
                return true;
            }
            return false;
        }

        // Eşleşmeleri işle
        private async void HandleMatches(List<Tile> matches)
        {
            // Puan ekle
            int matchScore = matches.Count * MATCH_SCORE;
            UpdateScore(matchScore);

            // Eşleşen kutucukları patlatıyoruz
            foreach (var tile in matches)
            {
                tile.OnMatched();
                await AnimateExplosion(tile.PictureBox); // Patlama animasyonu
            }

            // Patlatılan kutucukların bayraklarını sıfırlıyoruz
            foreach (var tile in matches)
            {
                tile.IsDestroyed = false;  // Bayrağı sıfırlıyoruz
            }

            // Yukarıdaki kutucukları düşür
            DropTiles();

            // Boş yerleri doldur
            FillEmptySpaces();

            // Yeniden eşleşme kontrolü
            await Task.Delay(300);
            if (CheckAndHandleMatches())
            {
                UpdateScore(50); // Kombinasyon bonusu
            }
        }

        // Patlama animasyonu
        private async Task AnimateExplosion(PictureBox pictureBox)
        {
            for (int i = 0; i < 5; i++)
            {
                pictureBox.Visible = !pictureBox.Visible; // Görünürlüğü değiştir
                await Task.Delay(50);
            }
            pictureBox.Visible = true; // Görünür yap
        }

        // Kutucukları aşağı düşürme
        private void DropTiles()
        {
            for (int col = 0; col < GRID_SIZE; col++)
            {
                int emptyRow = GRID_SIZE - 1;

                for (int row = GRID_SIZE - 1; row >= 0; row--)
                {
                    if (gameTiles[row, col] != null && gameTiles[row, col].Color != Color.White)
                    {
                        if (emptyRow != row)
                        {
                            // Kutucuğu aşağıya taşıyoruz
                            gameTiles[emptyRow, col] = gameTiles[row, col];
                            gameTiles[emptyRow, col].Position = new Point(emptyRow, col);
                            gameTiles[emptyRow, col].PictureBox.Location = new Point(col * TILE_SIZE + 1, emptyRow * TILE_SIZE + 1);

                            // Tag özelliğini güncelliyoruz
                            gameTiles[emptyRow, col].PictureBox.Tag = new Point(emptyRow, col);

                            gameTiles[row, col] = null;
                        }
                        emptyRow--;
                    }
                    else
                    {
                        if (gameTiles[row, col] != null)
                        {
                            gamePanel.Controls.Remove(gameTiles[row, col].PictureBox);
                            gameTiles[row, col].PictureBox.Dispose();
                            gameTiles[row, col] = null;
                        }
                    }
                }
            }
        }

        // Boş yerleri yeni kutucuklarla doldurma
        private void FillEmptySpaces()
        {
            for (int col = 0; col < GRID_SIZE; col++)
            {
                for (int row = 0; row < GRID_SIZE; row++)
                {
                    if (gameTiles[row, col] == null || gameTiles[row, col].Color == Color.White)
                    {
                        if (gameTiles[row, col] != null)
                        {
                            // Eski PictureBox'ı kaldırıyoruz
                            gamePanel.Controls.Remove(gameTiles[row, col].PictureBox);
                            gameTiles[row, col].PictureBox.Dispose();
                        }

                        PictureBox pictureBox = CreatePictureBox(row, col); // Yeni PictureBox oluştur
                        gamePanel.Controls.Add(pictureBox);

                        Color newColor = GetRandomColor(); // Yeni renk seç
                        Tile newTile = new NormalTile(newColor, pictureBox, new Point(row, col), gameTiles, random); // Yeni kutucuk oluştur
                        gameTiles[row, col] = newTile;
                        pictureBox.BackColor = newColor;

                        pictureBox.Tag = new Point(row, col);
                        pictureBox.Click += Tile_Click;
                    }
                }
            }
        }

        // Oyuncunun puanını günceller
        private void UpdateScore(int points)
        {
            score += points;
            lblScore.Text = $"Puan: {score}";

            // Bonus süre ekliyoruz
            if (points >= 100)
            {
                timeLeft += 10;
                lblTime.Text = $"Kalan Süre(Sn): {timeLeft}";
            }
        }

        // Oyun zamanlayıcısı
        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (isPaused) return;

            timeLeft--;
            lblTime.Text = $"Kalan Süre(Sn): {timeLeft}";

            if (timeLeft <= 0)
            {
                EndGame(); // Oyun biter
            }
        }

        // Oyunu sonlandırma
        private void EndGame()
        {
            gameTimer.Stop();
            CheckHighScore(); // En yüksek skoru kontrol et
            MessageBox.Show($"Oyun Bitti!\nToplam Puan: {score}", "Oyun Sonu");
            this.Close();
        }

        // En yüksek skorları yükler
        private void LoadHighScores()
        {
            highScores.Clear();
            if (File.Exists(HIGH_SCORES_FILE))
            {
                string[] lines = File.ReadAllLines(HIGH_SCORES_FILE);
                foreach (string line in lines)
                {
                    string[] parts = line.Split(',');
                    if (parts.Length == 2 && int.TryParse(parts[1], out int score))
                    {
                        highScores.Add(new KeyValuePair<string, int>(parts[0], score));
                    }
                }
            }
            highScores = highScores.OrderByDescending(x => x.Value).Take(5).ToList();
        }

        // En yüksek skorları kaydeder
        private void SaveHighScores()
        {
            using (StreamWriter writer = new StreamWriter(HIGH_SCORES_FILE))
            {
                foreach (var score in highScores)
                {
                    writer.WriteLine($"{score.Key},{score.Value}");
                }
            }
        }

        // En yüksek skorları kontrol eder ve ekler
        private void CheckHighScore()
        {
            LoadHighScores();

            if (highScores.Count < 5 || score > highScores.Last().Value)
            {
                string playerName = Form1.PlayerName;
                highScores.Add(new KeyValuePair<string, int>(playerName, score));
                highScores = highScores.OrderByDescending(x => x.Value).Take(5).ToList();
                SaveHighScores();
                ShowHighScores();
            }
        }

        // En yüksek skorları gösterir
        private void ShowHighScores()
        {
            // En iyi skorları güncel olarak yükleyelim
            LoadHighScores();

            Form highScoresForm = new Form
            {
                Text = "En İyi Skorlar",
                Size = new Size(300, 400),
                StartPosition = FormStartPosition.CenterScreen
            };

            ListBox highScoresList = new ListBox
            {
                Dock = DockStyle.Fill
            };

            foreach (var score in highScores)
            {
                highScoresList.Items.Add($"{score.Key}: {score.Value}");
            }

            highScoresForm.Controls.Add(highScoresList);
            highScoresForm.ShowDialog();
        }

        // Oyunu duraklatma
        private void PauseGame()
        {
            if (!isPaused)
            {
                isPaused = true;
                gameTimer.Stop();
                MessageBox.Show("Oyun Duraklatıldı\nDevam etmek için OK'a basın", "Pause");
                isPaused = false;
                gameTimer.Start();
            }
        }

        // Klavye kontrolleri
        private void Form2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.P)
            {
                PauseGame();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                DeselectTile();
            }
            else if (!isPaused)
            {
                HandleKeyboardInput(e.KeyCode);
            }
        }

        private void HandleKeyboardInput(Keys key)
        {
            if (selectedTile == null)
            {
                // Henüz bir kutucuk seçilmediyse en üst soldaki kutucuğu seçelim
                SelectTile(gameTiles[0, 0]);
                return;
            }

            Point pos = selectedTile.Position;
            int newRow = pos.X;
            int newCol = pos.Y;

            switch (key)
            {
                case Keys.Left:
                case Keys.A:
                    newCol = pos.Y - 1;
                    break;
                case Keys.Right:
                case Keys.D:
                    newCol = pos.Y + 1;
                    break;
                case Keys.Up:
                case Keys.W:
                    newRow = pos.X - 1;
                    break;
                case Keys.Down:
                case Keys.S:
                    newRow = pos.X + 1;
                    break;
                default:
                    return;
            }

            // Yeni pozisyon oyun alanı içinde mi kontrol et
            if (newRow >= 0 && newRow < GRID_SIZE && newCol >= 0 && newCol < GRID_SIZE)
            {
                Tile adjacentTile = gameTiles[newRow, newCol];
                SwapTiles(selectedTile, adjacentTile);

                // Eşleşme kontrolü
                if (!CheckAndHandleMatches())
                {
                    // Eşleşme yoksa değişikliği geri al
                    SwapTiles(selectedTile, adjacentTile);
                }

                DeselectTile();
            }
        }

        // Form yüklendiğinde tetiklenen olay
        private void Form2_Load(object sender, EventArgs e)
        {
            lbl_Playername.Text = "Oyuncu: " + Form1.PlayerName;
            lblTime.Text = "Kalan Süre(Sn): " + timeLeft.ToString();
            lblScore.Text = "Puan: " + score.ToString();
        }
    }
}
