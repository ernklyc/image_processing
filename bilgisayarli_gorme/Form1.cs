using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace bilgisayarli_gorme
{
    public partial class Form1 : Form
    {
        private List<int> merkezler = new List<int>();
        private Dictionary<int, List<int>> kümeler = new Dictionary<int, List<int>>();
        
        //******************************************
        //*       FORM BAŞLANGIÇ AYARLARI        //*
        //******************************************
        public Form1()
        {
            InitializeComponent();
            // ComboBox'a seçenekleri ekle
            comboBox1.Items.Add("Gri Yap");
            comboBox1.Items.Add("Y ile Gri Yap");
            comboBox1.Items.Add("K-Means Intensity");
            comboBox1.Items.Add("Sobel Kenar Bulma");
            comboBox1.Items.Add("Histogram");
            comboBox1.SelectedIndex = 0;

            // ComboBox2'ye tepe değerlerini ekle
            for (int i = 1; i <= 10; i++)
            {
                comboBox2.Items.Add(i.ToString());
            }
            comboBox2.SelectedIndex = 0;
        }


        //******************************************
        //*         RESİM SEÇME BÖLÜMÜ           //*
        //******************************************
        private void btnResimSec_Click(object sender, EventArgs e)
        {
            try
            {
                // Dosya seçme penceresini hazırla
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "Resim Dosyaları|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                ofd.Title = "Bir Resim Dosyası Seçin";

                // Eğer kullanıcı bir resim seçtiyse
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    // Eğer önceden bir resim varsa bellekten temizle
                    if (pictureBox1.Image != null)
                    {
                        pictureBox1.Image.Dispose();
                        pictureBox1.Image = null;
                    }

                    // Yeni resmi PictureBox1'e yükle
                    pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                    pictureBox1.Image = Image.FromFile(ofd.FileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Resim yüklenirken bir hata oluştu: " + ex.Message, 
                              "Hata", 
                              MessageBoxButtons.OK, 
                              MessageBoxIcon.Error);
            }
        }

        //******************************************
        //*         GÖRÜNTÜ İŞLEMLERİ            //*
        //******************************************
        
        // Basit Gri Tonlama İşlemi
        private void GriTonlamayaCevir()
        {
            if (pictureBox1.Image == null) return;

            Bitmap kaynak = new Bitmap(pictureBox1.Image);
            Bitmap hedef = new Bitmap(kaynak.Width, kaynak.Height);

            // Her piksel için tek tek işlem yap
            for (int x = 0; x < kaynak.Width; x++)
            {
                for (int y = 0; y < kaynak.Height; y++)
                {
                    Color piksel = kaynak.GetPixel(x, y);
                    int griDeger = (piksel.R + piksel.G + piksel.B) / 3;
                    Color yeniPiksel = Color.FromArgb(piksel.A, griDeger, griDeger, griDeger);
                    hedef.SetPixel(x, y, yeniPiksel);
                }
            }

            if (pictureBox2.Image != null)
            {
                pictureBox2.Image.Dispose();
            }
            pictureBox2.Image = hedef;
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
        }
        //******************************************
        // Y Değeri ile Gri Tonlama İşlemi       //*
        //******************************************
        private void YDegeriyleGriYap()
        {
            if (pictureBox1.Image == null) return;

            Bitmap kaynak = new Bitmap(pictureBox1.Image);
            Bitmap hedef = new Bitmap(kaynak.Width, kaynak.Height);

            // Her piksel için tek tek işlem yap
            for (int x = 0; x < kaynak.Width; x++)
            {
                for (int y = 0; y < kaynak.Height; y++)
                {
                    Color piksel = kaynak.GetPixel(x, y);
                    
                    // Y = 0.299R + 0.587G + 0.114B formülü ile hesaplama
                    int yDegeri = (int)((0.299 * piksel.R) + (0.587 * piksel.G) + (0.114 * piksel.B));
                    
                    // Değerin 0-255 aralığında olduğundan emin ol
                    yDegeri = Math.Max(0, Math.Min(255, yDegeri));
                    
                    Color yeniPiksel = Color.FromArgb(piksel.A, yDegeri, yDegeri, yDegeri);
                    hedef.SetPixel(x, y, yeniPiksel);
                }
            }

            if (pictureBox2.Image != null)
            {
                pictureBox2.Image.Dispose();
            }
            pictureBox2.Image = hedef;
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
        }

        //******************************************
        //*         K-Means Intensity İşlemi     //*
        //******************************************
        private void KMeansIntensity()
        {
            if (pictureBox1.Image == null) return;

            Bitmap kaynak = new Bitmap(pictureBox1.Image);
            Bitmap hedef = new Bitmap(kaynak.Width, kaynak.Height);
            
            // Gri tonlama değerlerini al
            List<int> griDegerler = new List<int>();
            for (int x = 0; x < kaynak.Width; x++)
            {
                for (int y = 0; y < kaynak.Height; y++)
                {
                    Color piksel = kaynak.GetPixel(x, y);
                    int griDeger = (piksel.R + piksel.G + piksel.B) / 3;
                    griDegerler.Add(griDeger);
                }
            }

            // Histogram verilerini hazırla
            int[] histogram = new int[256];
            foreach (int deger in griDegerler)
            {
                histogram[deger]++;
            }

            // Chart'ı temizle ve ayarla
            chart1.Series.Clear();
            chart1.ChartAreas[0].AxisX.Minimum = 0;
            chart1.ChartAreas[0].AxisX.Maximum = 255;
            chart1.ChartAreas[0].AxisX.Title = "Gri Seviye";
            chart1.ChartAreas[0].AxisY.Title = "Piksel Sayısı";

            // Histogram serisini ekle
            var histogramSeries = new System.Windows.Forms.DataVisualization.Charting.Series();
            histogramSeries.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column;
            histogramSeries.Color = Color.Gray;
            histogramSeries.Name = "Histogram";

            // Histogram verilerini ekle
            for (int i = 0; i < 256; i++)
            {
                histogramSeries.Points.AddXY(i, histogram[i]);
            }
            chart1.Series.Add(histogramSeries);

            // K değerini al
            int k = int.Parse(comboBox2.SelectedItem.ToString());
            label8.Text = k.ToString();  // T değerini göster

            // Başlangıç merkezlerini rastgele seç
            Random rnd = new Random();
            merkezler.Clear();
            for (int i = 0; i < k; i++)
            {
                int rastgeleMerkez = griDegerler[rnd.Next(griDegerler.Count)];
                merkezler.Add(rastgeleMerkez);
            }

            // ListView2'yi hazırla ve başlangıç değerlerini göster
            listView2.Clear();
            listView2.Columns.Add("T", 50);
            listView2.Columns.Add("Değer", 100);
            
            // Başlangıç değerlerini sıralı olarak göster
            var sıraliBaslangicDegerler = merkezler.Select((deger, index) => new { Deger = deger, Index = index })
                                                 .OrderBy(x => x.Deger)
                                                 .ToList();

            foreach (var merkez in sıraliBaslangicDegerler)
            {
                ListViewItem item = new ListViewItem($"T{merkez.Index + 1}");
                item.SubItems.Add(merkez.Deger.ToString());
                listView2.Items.Add(item);
            }

            // K-means iterasyonları
            bool değişimVar;
            int maxIterasyon = 100;
            int iterasyon = 0;

            do
            {
                değişimVar = false;
                
                // Kümeleri temizle
                kümeler.Clear();
                for (int i = 0; i < k; i++)
                {
                    kümeler[i] = new List<int>();
                }

                // Her pikseli en yakın kümeye ata
                foreach (int griDeger in griDegerler)
                {
                    int enYakınKüme = 0;
                    int enKüçükUzaklık = int.MaxValue;

                    for (int i = 0; i < k; i++)
                    {
                        int uzaklık = Math.Abs(griDeger - merkezler[i]);
                        if (uzaklık < enKüçükUzaklık)
                        {
                            enKüçükUzaklık = uzaklık;
                            enYakınKüme = i;
                        }
                    }

                    kümeler[enYakınKüme].Add(griDeger);
                }

                // Yeni merkez noktalarını hesapla
                for (int i = 0; i < k; i++)
                {
                    if (kümeler[i].Count > 0)
                    {
                        int yeniMerkez = (int)kümeler[i].Average();
                        if (yeniMerkez != merkezler[i])
                        {
                            değişimVar = true;
                            merkezler[i] = yeniMerkez;
                        }
                    }
                }

                iterasyon++;
                label4.Text = iterasyon.ToString();  // İterasyon sayısını güncelle

            } while (değişimVar && iterasyon < maxIterasyon);

            // Toplam piksel sayısını göster
            label6.Text = griDegerler.Count.ToString();

            // Sonuçları listbox'a ekle
            listBox2.Items.Clear();
            
            // Merkezleri sırala
            var sıraliMerkezler = merkezler.Select((deger, index) => new { Deger = deger, Index = index })
                                         .OrderBy(x => x.Deger)
                                         .ToList();

            foreach (var merkez in sıraliMerkezler)
            {
                listBox2.Items.Add($"{kümeler[merkez.Index].Count}px T{merkez.Index + 1}={merkez.Deger} ({merkez.Deger},{merkez.Deger},{merkez.Deger})");
            }

            // Görüntüyü güncelle
            for (int x = 0; x < kaynak.Width; x++)
            {
                for (int y = 0; y < kaynak.Height; y++)
                {
                    Color piksel = kaynak.GetPixel(x, y);
                    int griDeger = (piksel.R + piksel.G + piksel.B) / 3;
                    
                    // En yakın küme merkezini bul
                    int enYakınMerkez = merkezler[0];
                    int enKüçükUzaklık = Math.Abs(griDeger - merkezler[0]);

                    for (int i = 1; i < k; i++)
                    {
                        int uzaklık = Math.Abs(griDeger - merkezler[i]);
                        if (uzaklık < enKüçükUzaklık)
                        {
                            enKüçükUzaklık = uzaklık;
                            enYakınMerkez = merkezler[i];
                        }
                    }

                    Color yeniPiksel = Color.FromArgb(piksel.A, enYakınMerkez, enYakınMerkez, enYakınMerkez);
                    hedef.SetPixel(x, y, yeniPiksel);
                }
            }

            if (pictureBox2.Image != null)
            {
                pictureBox2.Image.Dispose();
            }
            pictureBox2.Image = hedef;
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;

            // K-means tamamlandıktan sonra tepe noktalarını işaretle
            var tepeSeries = new System.Windows.Forms.DataVisualization.Charting.Series();
            tepeSeries.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
            tepeSeries.Color = Color.Red;
            tepeSeries.Name = "Tepe Noktaları";
            tepeSeries.MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Circle;
            tepeSeries.MarkerSize = 10;

            // Sıralı merkez değerlerini tepe noktası olarak işaretle
            foreach (var merkez in merkezler.OrderBy(x => x))
            {
                tepeSeries.Points.AddXY(merkez, histogram[merkez]);
            }
            chart1.Series.Add(tepeSeries);
        }

        //******************************************
        //*         SOBEL KENAR BULMA            //*
        //******************************************
        private void SobelKenarBulma()
        {
            if (pictureBox1.Image == null) return;

            Bitmap kaynak = new Bitmap(pictureBox1.Image);
            Bitmap hedef = new Bitmap(kaynak.Width, kaynak.Height);

            // Sobel operatörleri
            int[,] Gx = new int[,] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
            int[,] Gy = new int[,] { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } };

            // Önce gri tonlamaya çevir
            int[,] griResim = new int[kaynak.Width, kaynak.Height];
            int[,] kenarlar = new int[kaynak.Width, kaynak.Height];

            // Resmi gri tonlamaya çevir
            for (int x = 0; x < kaynak.Width; x++)
            {
                for (int y = 0; y < kaynak.Height; y++)
                {
                    Color piksel = kaynak.GetPixel(x, y);
                    griResim[x, y] = (piksel.R + piksel.G + piksel.B) / 3;
                }
            }

            // Sobel operatörünü uygula
            for (int x = 1; x < kaynak.Width - 1; x++)
            {
                for (int y = 1; y < kaynak.Height - 1; y++)
                {
                    int px = 0, py = 0;

                    // 3x3 komşuluk için Gx ve Gy'yi uygula
                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            px += griResim[x + i, y + j] * Gx[i + 1, j + 1];
                            py += griResim[x + i, y + j] * Gy[i + 1, j + 1];
                        }
                    }

                    // Gradyan büyüklüğünü hesapla
                    int gradyan = (int)Math.Sqrt(px * px + py * py);
                    
                    // 255'e normalize et
                    gradyan = Math.Min(255, gradyan);
                    
                    kenarlar[x, y] = gradyan;
                }
            }

            // Sonucu Bitmap'e aktar
            for (int x = 0; x < kaynak.Width; x++)
            {
                for (int y = 0; y < kaynak.Height; y++)
                {
                    int kenarDegeri = x == 0 || y == 0 || x == kaynak.Width - 1 || y == kaynak.Height - 1 ? 0 : kenarlar[x, y];
                    Color yeniPiksel = Color.FromArgb(255, kenarDegeri, kenarDegeri, kenarDegeri);
                    hedef.SetPixel(x, y, yeniPiksel);
                }
            }

            if (pictureBox2.Image != null)
            {
                pictureBox2.Image.Dispose();
            }
            pictureBox2.Image = hedef;
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
        }

        //******************************************
        //*         HISTOGRAM ÇİZME               //*
        //******************************************
        private void HistogramCizme()
        {
            if (pictureBox1.Image == null) return;

            Bitmap kaynak = new Bitmap(pictureBox1.Image);
            Bitmap hedef = new Bitmap(kaynak.Width, kaynak.Height);
            
            // Gri tonlama değerlerini al
            List<int> griDegerler = new List<int>();
            for (int x = 0; x < kaynak.Width; x++)
            {
                for (int y = 0; y < kaynak.Height; y++)
                {
                    Color piksel = kaynak.GetPixel(x, y);
                    int griDeger = (piksel.R + piksel.G + piksel.B) / 3;
                    griDegerler.Add(griDeger);
                    
                    // Gri görüntüyü de oluştur
                    Color yeniPiksel = Color.FromArgb(piksel.A, griDeger, griDeger, griDeger);
                    hedef.SetPixel(x, y, yeniPiksel);
                }
            }

            // Histogram verilerini hazırla
            int[] histogram = new int[256];
            foreach (int deger in griDegerler)
            {
                histogram[deger]++;
            }

            // Chart'ı temizle ve ayarla
            chart1.Series.Clear();
            chart1.ChartAreas[0].AxisX.Minimum = 0;
            chart1.ChartAreas[0].AxisX.Maximum = 255;
            chart1.ChartAreas[0].AxisX.Title = "Gri Seviye";
            chart1.ChartAreas[0].AxisY.Title = "Piksel Sayısı";

            // Histogram serisini ekle
            var histogramSeries = new System.Windows.Forms.DataVisualization.Charting.Series();
            histogramSeries.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column;
            histogramSeries.Color = Color.Gray;
            histogramSeries.Name = "Histogram";

            // Histogram verilerini ekle
            for (int i = 0; i < 256; i++)
            {
                histogramSeries.Points.AddXY(i, histogram[i]);
            }
            chart1.Series.Add(histogramSeries);

            // Gri görüntüyü göster
            if (pictureBox2.Image != null)
            {
                pictureBox2.Image.Dispose();
            }
            pictureBox2.Image = hedef;
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
        }

        //******************************************
        //*         İŞLEM BAŞLATMA BÖLÜMÜ        //*
        //******************************************
        private void button2_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("Lütfen önce bir resim seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                switch (comboBox1.SelectedItem.ToString())
                {
                    case "Gri Yap":
                        GriTonlamayaCevir();
                        break;
                    case "Y ile Gri Yap":
                        YDegeriyleGriYap();
                        break;
                    case "K-Means Intensity":
                        KMeansIntensity();
                        break;
                    case "Sobel Kenar Bulma":
                        SobelKenarBulma();
                        break;
                    case "Histogram":
                        HistogramCizme();
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("İşlem sırasında hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
