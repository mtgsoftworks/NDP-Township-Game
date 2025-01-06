using System;                           // Temel sistem işlevlerini içeren ad alanı.
using System.Collections.Generic;       // Koleksiyonlar ve generics (genel türler) için gerekli ad alanı.
using System.ComponentModel;            // Bileşen modelini desteklemek için kullanılan ad alanı.
using System.Data;                      // Veri erişim ve yönetimi için kullanılan ad alanı.
using System.Drawing;                   // Grafiksel çizim işlemleri için gerekli ad alanı.
using System.IO;                        // Giriş/Çıkış işlemleri (dosya okuma/yazma vb.) için gereken ad alanı.
using System.Linq;                      // LINQ sorguları ve operasyonları için gerekli ad alanı.
using System.Text;                      // Metin ile ilgili işlemler için kullanılan ad alanı.
using System.Threading.Tasks;           // Çok iş parçacıklı ve asenkron programlama için kullanılan ad alanı.
using System.Windows.Forms;             // Windows Forms uygulamaları geliştirmek için kullanılan ad alanı.

namespace NDP_Township_GAME              // Projenin ad alanı; kodu organize etmek için kullanılır.
{
    // Windows Form'dan türetilen Form1 sınıfı; uygulamanın ana formunu temsil eder.
    public partial class Form1 : Form
    {
        // Form1 sınıfının yapıcı (constructor) metodu; form ilk oluşturulduğunda çalışır.
        public Form1()
        {
            InitializeComponent();       // Form üzerindeki tüm bileşenlerin başlatılmasını sağlar.
        }

        // Oyuncu ismini tutmak için tanımlanan statik özellik (property).
        // Statik olduğu için sınıfın herhangi bir örneği olmadan erişilebilir.
        public static string PlayerName { get; set; }

        private const string HIGH_SCORES_FILE = "highscores.txt";  // En yüksek puanların saklanacağı dosyanın adı.

        // En yüksek puanları tutmak için kullanılan liste.
        // Her bir öğe, oyuncu ismi ve puanı içeren bir anahtar-değer çiftidir.
        private List<KeyValuePair<string, int>> highScores = new List<KeyValuePair<string, int>>();

        // En yüksek puanları ekranda göstermek için kullanılan metot.
        private void ShowHighScores()
        {
            // En yüksek puanları dosyadan yükle.
            LoadHighScores();

            // Yeni bir form oluştur; bu form en iyi skorları gösterecek.
            Form highScoresForm = new Form
            {
                Text = "En İyi Skorlar",                           // Formun başlığı.
                Size = new Size(300, 400),                         // Formun boyutu (genişlik x yükseklik).
                StartPosition = FormStartPosition.CenterScreen     // Formun ekranda ortalanmasını sağlar.
            };

            // Puanları listelemek için bir ListBox kontrolü oluştur.
            ListBox highScoresList = new ListBox
            {
                Dock = DockStyle.Fill                              // ListBox'ın formu tamamen kaplamasını sağlar.
            };

            // En yüksek puanlar listesindeki her bir skoru ListBox'a ekle.
            foreach (var score in highScores)
            {
                highScoresList.Items.Add($"{score.Key}: {score.Value}"); // Örnek format: "OyuncuAdi: Puan"
            }

            // ListBox'ı forma ekle.
            highScoresForm.Controls.Add(highScoresList);

            // En iyi skorları gösteren formu ekranda göster.
            highScoresForm.ShowDialog();
        }

        // En yüksek puanları dosyadan yüklemek için kullanılan metot.
        private void LoadHighScores()
        {
            highScores.Clear();    // Mevcut puanları temizle.

            // Eğer puanların saklandığı dosya mevcutsa:
            if (File.Exists(HIGH_SCORES_FILE))
            {
                string[] lines = File.ReadAllLines(HIGH_SCORES_FILE); // Dosyadaki tüm satırları oku.

                foreach (string line in lines) // Her bir satır için:
                {
                    string[] parts = line.Split(','); // Satırı virgül ile ayırarak parçalara böl.

                    // Eğer satır doğru formatta ve puan kısmı geçerliyse:
                    if (parts.Length == 2 && int.TryParse(parts[1], out int score))
                    {
                        // Listeye oyuncu adını ve puanını ekle.
                        highScores.Add(new KeyValuePair<string, int>(parts[0], score));
                    }
                }
            }

            // Puanları büyükten küçüğe doğru sırala ve ilk 5 tanesini al.
            highScores = highScores.OrderByDescending(x => x.Value).Take(5).ToList();
        }

        // "BAŞLA" butonuna tıklandığında tetiklenen olay metodu.
        private void button1_Click(object sender, EventArgs e)
        {
            // Oyuncunun ismini textBox1'den al ve PlayerName özelliğine ata.
            PlayerName = textBox1.Text;

            // Eğer oyuncu ismi boş değilse:
            if (!string.IsNullOrEmpty(PlayerName))
            {
                // Yeni bir oyun formu (Form2) oluştur.
                Form2 gameForm = new Form2();

                // Oyun formunu ekranda göster.
                gameForm.Show();

                // Mevcut formu gizle (arka planda çalışmaya devam eder).
                this.Hide();
            }
            else
            {
                // Eğer oyuncu ismi girilmemişse, kullanıcıya uyarı mesajı göster.
                MessageBox.Show("Lütfen adınızı girin!");
            }
        }

        // Form1 yüklendiğinde tetiklenen olay metodu.
        private void Form1_Load(object sender, EventArgs e)
        {
            // label3 kontrolünün metin özelliğini ayarla.
            label3.Text = "Oyunu başlatmak için BAŞLA butonuna basınız.";
        }

        // "En İyi Skorlar" linkine tıklandığında tetiklenen olay metodu.
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // En yüksek puanları gösteren metotu çağır.
            ShowHighScores();
        }
    }
}

