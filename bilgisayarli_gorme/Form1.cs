﻿using System;
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
            comboBox1.Items.Add("K-Means RGB");
            comboBox1.Items.Add("K-Means Mahalanobis");
            comboBox1.Items.Add("Sobel Kenar Bulma");
            comboBox1.Items.Add("Histogram");
            comboBox1.Items.Add("K-Means Mahalanobis ND");
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
        //*         K-Means RGB İşlemi           //*
        //******************************************
        private class RGBPoint
        {
            public int R, G, B;
            public RGBPoint(int r, int g, int b)
            {
                R = r;
                G = g;
                B = b;
            }

            // İki nokta arasındaki Öklid mesafesini hesapla
            public double Distance(RGBPoint other)
            {
                int dr = R - other.R;
                int dg = G - other.G;
                int db = B - other.B;
                return Math.Sqrt(dr * dr + dg * dg + db * db);
            }
        }

        private void KMeansRGB()
        {
            if (pictureBox1.Image == null) return;

            Bitmap kaynak = new Bitmap(pictureBox1.Image);
            Bitmap hedef = new Bitmap(kaynak.Width, kaynak.Height);

            // RGB noktalarını al
            List<RGBPoint> noktalar = new List<RGBPoint>();
            for (int x = 0; x < kaynak.Width; x++)
            {
                for (int y = 0; y < kaynak.Height; y++)
                {
                    Color piksel = kaynak.GetPixel(x, y);
                    noktalar.Add(new RGBPoint(piksel.R, piksel.G, piksel.B));
                }
            }

            // K değerini al
            int k = int.Parse(comboBox2.SelectedItem.ToString());
            label8.Text = k.ToString();

            // Başlangıç merkezlerini rastgele seç
            Random rnd = new Random();
            List<RGBPoint> merkezler = new List<RGBPoint>();
            for (int i = 0; i < k; i++)
            {
                int rastgeleIndex = rnd.Next(noktalar.Count);
                RGBPoint merkez = noktalar[rastgeleIndex];
                merkezler.Add(new RGBPoint(merkez.R, merkez.G, merkez.B));
            }

            // ListView2'yi hazırla ve başlangıç değerlerini göster
            listView2.Clear();
            listView2.Columns.Add("T", 50);
            listView2.Columns.Add("R", 50);
            listView2.Columns.Add("G", 50);
            listView2.Columns.Add("B", 50);

            for (int i = 0; i < k; i++)
            {
                ListViewItem item = new ListViewItem($"T{i + 1}");
                item.SubItems.Add(merkezler[i].R.ToString());
                item.SubItems.Add(merkezler[i].G.ToString());
                item.SubItems.Add(merkezler[i].B.ToString());
                listView2.Items.Add(item);
            }

            // K-means iterasyonları
            bool değişimVar;
            int maxIterasyon = 100;
            int iterasyon = 0;
            Dictionary<int, List<RGBPoint>> kümeler = new Dictionary<int, List<RGBPoint>>();

            do
            {
                değişimVar = false;

                // Kümeleri temizle
                kümeler.Clear();
                for (int i = 0; i < k; i++)
                {
                    kümeler[i] = new List<RGBPoint>();
                }

                // Her noktayı en yakın kümeye ata
                foreach (var nokta in noktalar)
                {
                    int enYakınKüme = 0;
                    double enKüçükUzaklık = nokta.Distance(merkezler[0]);

                    for (int i = 1; i < k; i++)
                    {
                        double uzaklık = nokta.Distance(merkezler[i]);
                        if (uzaklık < enKüçükUzaklık)
                        {
                            enKüçükUzaklık = uzaklık;
                            enYakınKüme = i;
                        }
                    }

                    kümeler[enYakınKüme].Add(nokta);
                }

                // Yeni merkez noktalarını hesapla
                for (int i = 0; i < k; i++)
                {
                    if (kümeler[i].Count > 0)
                    {
                        int yeniR = (int)kümeler[i].Average(p => p.R);
                        int yeniG = (int)kümeler[i].Average(p => p.G);
                        int yeniB = (int)kümeler[i].Average(p => p.B);

                        if (yeniR != merkezler[i].R || yeniG != merkezler[i].G || yeniB != merkezler[i].B)
                        {
                            değişimVar = true;
                            merkezler[i] = new RGBPoint(yeniR, yeniG, yeniB);
                        }
                    }
                }

                iterasyon++;
                label4.Text = iterasyon.ToString();

            } while (değişimVar && iterasyon < maxIterasyon);

            // Toplam piksel sayısını göster
            label6.Text = noktalar.Count.ToString();

            // Sonuçları listbox'a ekle
            listBox2.Items.Clear();
            for (int i = 0; i < k; i++)
            {
                listBox2.Items.Add($"{kümeler[i].Count}px T{i + 1}=({merkezler[i].R},{merkezler[i].G},{merkezler[i].B})");
            }

            // Görüntüyü güncelle
            for (int x = 0; x < kaynak.Width; x++)
            {
                for (int y = 0; y < kaynak.Height; y++)
                {
                    Color piksel = kaynak.GetPixel(x, y);
                    RGBPoint nokta = new RGBPoint(piksel.R, piksel.G, piksel.B);

                    // En yakın merkezi bul
                    int enYakınKüme = 0;
                    double enKüçükUzaklık = nokta.Distance(merkezler[0]);

                    for (int i = 1; i < k; i++)
                    {
                        double uzaklık = nokta.Distance(merkezler[i]);
                        if (uzaklık < enKüçükUzaklık)
                        {
                            enKüçükUzaklık = uzaklık;
                            enYakınKüme = i;
                        }
                    }

                    // Pikseli en yakın merkezin rengiyle boya
                    Color yeniPiksel = Color.FromArgb(
                        piksel.A,
                        merkezler[enYakınKüme].R,
                        merkezler[enYakınKüme].G,
                        merkezler[enYakınKüme].B
                    );
                    hedef.SetPixel(x, y, yeniPiksel);
                }
            }

            if (pictureBox2.Image != null)
            {
                pictureBox2.Image.Dispose();
            }
            pictureBox2.Image = hedef;
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;

            // Histogram için gri değerleri hesapla ve göster
            chart1.Series.Clear();
            chart1.ChartAreas[0].AxisX.Minimum = 0;
            chart1.ChartAreas[0].AxisX.Maximum = 255;
            chart1.ChartAreas[0].AxisX.Title = "Gri Seviye";
            chart1.ChartAreas[0].AxisY.Title = "Piksel Sayısı";

            // Histogram serisi
            var histogramSeries = new System.Windows.Forms.DataVisualization.Charting.Series();
            histogramSeries.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column;
            histogramSeries.Color = Color.Gray;
            histogramSeries.Name = "Histogram";

            // Histogram verilerini hazırla
            int[] histogram = new int[256];
            foreach (var nokta in noktalar)
            {
                int griDeger = (nokta.R + nokta.G + nokta.B) / 3;
                histogram[griDeger]++;
            }

            // Verileri ekle
            for (int i = 0; i < 256; i++)
            {
                histogramSeries.Points.AddXY(i, histogram[i]);
            }
            chart1.Series.Add(histogramSeries);

            // Merkez noktalarını işaretle
            var merkezSeries = new System.Windows.Forms.DataVisualization.Charting.Series();
            merkezSeries.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
            merkezSeries.Color = Color.Red;
            merkezSeries.MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Circle;
            merkezSeries.MarkerSize = 10;

            foreach (var merkez in merkezler)
            {
                int griMerkez = (merkez.R + merkez.G + merkez.B) / 3;
                merkezSeries.Points.AddXY(griMerkez, histogram[griMerkez]);
            }
            chart1.Series.Add(merkezSeries);
        }

        //******************************************
        //*     K-Means Mahalanobis İşlemi       //*
        //******************************************
        private void KMeansMahalanobis()
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

            // Varyans hesapla
            double ortalama = griDegerler.Average();
            double varyans = griDegerler.Sum(d => Math.Pow(d - ortalama, 2)) / griDegerler.Count;
            
            // K değerini al
            int k = int.Parse(comboBox2.SelectedItem.ToString());
            label8.Text = k.ToString();

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
            Dictionary<int, List<int>> kümeler = new Dictionary<int, List<int>>();

            do
            {
                değişimVar = false;
                
                // Kümeleri temizle
                kümeler.Clear();
                for (int i = 0; i < k; i++)
                {
                    kümeler[i] = new List<int>();
                }

                // Her pikseli en yakın kümeye ata (Mahalanobis mesafesi kullanarak)
                foreach (int griDeger in griDegerler)
                {
                    int enYakınKüme = 0;
                    double enKüçükUzaklık = double.MaxValue;

                    for (int i = 0; i < k; i++)
                    {
                        // Mahalanobis mesafesi: |x-y| / √varyans
                        double uzaklık = Math.Abs(griDeger - merkezler[i]) / Math.Sqrt(varyans);
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
                label4.Text = iterasyon.ToString();

            } while (değişimVar && iterasyon < maxIterasyon);

            // Toplam piksel sayısını göster
            label6.Text = griDegerler.Count.ToString();

            // Sonuçları listbox'a ekle
            listBox2.Items.Clear();
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
                    
                    // En yakın merkezi bul (Mahalanobis mesafesi kullanarak)
                    int enYakınMerkez = merkezler[0];
                    double enKüçükUzaklık = Math.Abs(griDeger - merkezler[0]) / Math.Sqrt(varyans);

                    for (int i = 1; i < k; i++)
                    {
                        double uzaklık = Math.Abs(griDeger - merkezler[i]) / Math.Sqrt(varyans);
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

            // Histogram çiz
            chart1.Series.Clear();
            chart1.ChartAreas[0].AxisX.Minimum = 0;
            chart1.ChartAreas[0].AxisX.Maximum = 255;
            chart1.ChartAreas[0].AxisX.Title = "Gri Seviye";
            chart1.ChartAreas[0].AxisY.Title = "Piksel Sayısı";

            var histogramSeries = new System.Windows.Forms.DataVisualization.Charting.Series();
            histogramSeries.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column;
            histogramSeries.Color = Color.Gray;
            histogramSeries.Name = "Histogram";

            // Histogram verilerini hazırla
            int[] histogram = new int[256];
            foreach (int deger in griDegerler)
            {
                histogram[deger]++;
            }

            // Verileri ekle
            for (int i = 0; i < 256; i++)
            {
                histogramSeries.Points.AddXY(i, histogram[i]);
            }
            chart1.Series.Add(histogramSeries);

            // Merkez noktalarını işaretle
            var tepeSeries = new System.Windows.Forms.DataVisualization.Charting.Series();
            tepeSeries.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
            tepeSeries.Color = Color.Red;
            tepeSeries.Name = "Tepe Noktaları";
            tepeSeries.MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Circle;
            tepeSeries.MarkerSize = 10;

            foreach (var merkez in merkezler.OrderBy(x => x))
            {
                tepeSeries.Points.AddXY(merkez, histogram[merkez]);
            }
            chart1.Series.Add(tepeSeries);
        }

        //******************************************
        //*     K-Means Mahalanobis ND İşlemi    //*
        //******************************************
        private class RGBPoint3D
        {
            public double[] Values;
            public RGBPoint3D(double r, double g, double b)
            {
                Values = new double[] { r, g, b };
            }
        }

        private double[,] HesaplaKovaryansMatrisi(List<RGBPoint3D> noktalar)
        {
            int n = noktalar.Count;
            double[] ortalamalar = new double[3];
            double[,] kovaryans = new double[3, 3];

            // Ortalamalar
            for (int i = 0; i < 3; i++)
            {
                ortalamalar[i] = noktalar.Average(p => p.Values[i]);
            }

            // Kovaryans matrisi
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    kovaryans[i, j] = noktalar.Sum(p => 
                        (p.Values[i] - ortalamalar[i]) * (p.Values[j] - ortalamalar[j])) / (n - 1);
                }
            }

            return kovaryans;
        }

        private double[,] MatrisInverse(double[,] matrix)
        {
            int n = 3;
            double[,] result = new double[n, n];
            double det = 
                matrix[0, 0] * (matrix[1, 1] * matrix[2, 2] - matrix[1, 2] * matrix[2, 1]) -
                matrix[0, 1] * (matrix[1, 0] * matrix[2, 2] - matrix[1, 2] * matrix[2, 0]) +
                matrix[0, 2] * (matrix[1, 0] * matrix[2, 1] - matrix[1, 1] * matrix[2, 0]);

            double invDet = 1.0 / det;

            result[0, 0] = (matrix[1, 1] * matrix[2, 2] - matrix[1, 2] * matrix[2, 1]) * invDet;
            result[0, 1] = (matrix[0, 2] * matrix[2, 1] - matrix[0, 1] * matrix[2, 2]) * invDet;
            result[0, 2] = (matrix[0, 1] * matrix[1, 2] - matrix[0, 2] * matrix[1, 1]) * invDet;
            result[1, 0] = (matrix[1, 2] * matrix[2, 0] - matrix[1, 0] * matrix[2, 2]) * invDet;
            result[1, 1] = (matrix[0, 0] * matrix[2, 2] - matrix[0, 2] * matrix[2, 0]) * invDet;
            result[1, 2] = (matrix[0, 2] * matrix[1, 0] - matrix[0, 0] * matrix[1, 2]) * invDet;
            result[2, 0] = (matrix[1, 0] * matrix[2, 1] - matrix[1, 1] * matrix[2, 0]) * invDet;
            result[2, 1] = (matrix[0, 1] * matrix[2, 0] - matrix[0, 0] * matrix[2, 1]) * invDet;
            result[2, 2] = (matrix[0, 0] * matrix[1, 1] - matrix[0, 1] * matrix[1, 0]) * invDet;

            return result;
        }

        private double MahalanobisUzaklik(RGBPoint3D p1, RGBPoint3D p2, double[,] invKovaryans)
        {
            double[] diff = new double[3];
            for (int i = 0; i < 3; i++)
            {
                diff[i] = p1.Values[i] - p2.Values[i];
            }

            double uzaklik = 0;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    uzaklik += diff[i] * invKovaryans[i, j] * diff[j];
                }
            }

            return Math.Sqrt(uzaklik);
        }

        private void KMeansMahalanobisND()
        {
            if (pictureBox1.Image == null) return;

            Bitmap kaynak = new Bitmap(pictureBox1.Image);
            Bitmap hedef = new Bitmap(kaynak.Width, kaynak.Height);

            // RGB noktalarını al
            List<RGBPoint3D> noktalar = new List<RGBPoint3D>();
            for (int x = 0; x < kaynak.Width; x++)
            {
                for (int y = 0; y < kaynak.Height; y++)
                {
                    Color piksel = kaynak.GetPixel(x, y);
                    noktalar.Add(new RGBPoint3D(piksel.R, piksel.G, piksel.B));
                }
            }

            // Kovaryans matrisini hesapla
            double[,] kovaryans = HesaplaKovaryansMatrisi(noktalar);
            double[,] invKovaryans = MatrisInverse(kovaryans);

            // K değerini al
            int k = int.Parse(comboBox2.SelectedItem.ToString());
            label8.Text = k.ToString();

            // Başlangıç merkezlerini rastgele seç
            Random rnd = new Random();
            List<RGBPoint3D> merkezler = new List<RGBPoint3D>();
            for (int i = 0; i < k; i++)
            {
                int rastgeleIndex = rnd.Next(noktalar.Count);
                var nokta = noktalar[rastgeleIndex];
                merkezler.Add(new RGBPoint3D(nokta.Values[0], nokta.Values[1], nokta.Values[2]));
            }

            // ListView2'yi hazırla
            listView2.Clear();
            listView2.Columns.Add("T", 50);
            listView2.Columns.Add("R", 50);
            listView2.Columns.Add("G", 50);
            listView2.Columns.Add("B", 50);

            for (int i = 0; i < k; i++)
            {
                ListViewItem item = new ListViewItem($"T{i + 1}");
                item.SubItems.Add(((int)merkezler[i].Values[0]).ToString());
                item.SubItems.Add(((int)merkezler[i].Values[1]).ToString());
                item.SubItems.Add(((int)merkezler[i].Values[2]).ToString());
                listView2.Items.Add(item);
            }

            // K-means iterasyonları
            bool değişimVar;
            int maxIterasyon = 100;
            int iterasyon = 0;
            Dictionary<int, List<RGBPoint3D>> kümeler = new Dictionary<int, List<RGBPoint3D>>();

            do
            {
                değişimVar = false;
                kümeler.Clear();
                for (int i = 0; i < k; i++)
                {
                    kümeler[i] = new List<RGBPoint3D>();
                }

                // Her noktayı en yakın kümeye ata
                foreach (var nokta in noktalar)
                {
                    int enYakınKüme = 0;
                    double enKüçükUzaklık = MahalanobisUzaklik(nokta, merkezler[0], invKovaryans);

                    for (int i = 1; i < k; i++)
                    {
                        double uzaklık = MahalanobisUzaklik(nokta, merkezler[i], invKovaryans);
                        if (uzaklık < enKüçükUzaklık)
                        {
                            enKüçükUzaklık = uzaklık;
                            enYakınKüme = i;
                        }
                    }

                    kümeler[enYakınKüme].Add(nokta);
                }

                // Yeni merkez noktalarını hesapla
                for (int i = 0; i < k; i++)
                {
                    if (kümeler[i].Count > 0)
                    {
                        double[] yeniMerkez = new double[3];
                        for (int j = 0; j < 3; j++)
                        {
                            yeniMerkez[j] = kümeler[i].Average(p => p.Values[j]);
                        }

                        if (Math.Abs(yeniMerkez[0] - merkezler[i].Values[0]) > 0.001 ||
                            Math.Abs(yeniMerkez[1] - merkezler[i].Values[1]) > 0.001 ||
                            Math.Abs(yeniMerkez[2] - merkezler[i].Values[2]) > 0.001)
                        {
                            değişimVar = true;
                            merkezler[i] = new RGBPoint3D(yeniMerkez[0], yeniMerkez[1], yeniMerkez[2]);
                        }
                    }
                }

                iterasyon++;
                label4.Text = iterasyon.ToString();

            } while (değişimVar && iterasyon < maxIterasyon);

            // Toplam piksel sayısını göster
            label6.Text = noktalar.Count.ToString();

            // Sonuçları listbox'a ekle
            listBox2.Items.Clear();
            for (int i = 0; i < k; i++)
            {
                listBox2.Items.Add($"{kümeler[i].Count}px T{i + 1}=({(int)merkezler[i].Values[0]},{(int)merkezler[i].Values[1]},{(int)merkezler[i].Values[2]})");
            }

            // Görüntüyü güncelle
            for (int x = 0; x < kaynak.Width; x++)
            {
                for (int y = 0; y < kaynak.Height; y++)
                {
                    Color piksel = kaynak.GetPixel(x, y);
                    var nokta = new RGBPoint3D(piksel.R, piksel.G, piksel.B);

                    int enYakınKüme = 0;
                    double enKüçükUzaklık = MahalanobisUzaklik(nokta, merkezler[0], invKovaryans);

                    for (int i = 1; i < k; i++)
                    {
                        double uzaklık = MahalanobisUzaklik(nokta, merkezler[i], invKovaryans);
                        if (uzaklık < enKüçükUzaklık)
                        {
                            enKüçükUzaklık = uzaklık;
                            enYakınKüme = i;
                        }
                    }

                    Color yeniPiksel = Color.FromArgb(
                        piksel.A,
                        (int)merkezler[enYakınKüme].Values[0],
                        (int)merkezler[enYakınKüme].Values[1],
                        (int)merkezler[enYakınKüme].Values[2]
                    );
                    hedef.SetPixel(x, y, yeniPiksel);
                }
            }

            if (pictureBox2.Image != null)
            {
                pictureBox2.Image.Dispose();
            }
            pictureBox2.Image = hedef;
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;

            // Histogram için gri değerleri hesapla ve göster
            chart1.Series.Clear();
            chart1.ChartAreas[0].AxisX.Minimum = 0;
            chart1.ChartAreas[0].AxisX.Maximum = 255;
            chart1.ChartAreas[0].AxisX.Title = "Gri Seviye";
            chart1.ChartAreas[0].AxisY.Title = "Piksel Sayısı";

            var histogramSeries = new System.Windows.Forms.DataVisualization.Charting.Series();
            histogramSeries.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column;
            histogramSeries.Color = Color.Gray;
            histogramSeries.Name = "Histogram";

            int[] histogram = new int[256];
            foreach (var nokta in noktalar)
            {
                int griDeger = (int)((nokta.Values[0] + nokta.Values[1] + nokta.Values[2]) / 3);
                histogram[griDeger]++;
            }

            for (int i = 0; i < 256; i++)
            {
                histogramSeries.Points.AddXY(i, histogram[i]);
            }
            chart1.Series.Add(histogramSeries);

            var merkezSeries = new System.Windows.Forms.DataVisualization.Charting.Series();
            merkezSeries.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
            merkezSeries.Color = Color.Red;
            merkezSeries.MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Circle;
            merkezSeries.MarkerSize = 10;

            foreach (var merkez in merkezler)
            {
                int griMerkez = (int)((merkez.Values[0] + merkez.Values[1] + merkez.Values[2]) / 3);
                merkezSeries.Points.AddXY(griMerkez, histogram[griMerkez]);
            }
            chart1.Series.Add(merkezSeries);
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
                // Her işlemden önce chart'ı temizle
                chart1.Series.Clear();
                chart1.ChartAreas[0].AxisX.Minimum = 0;
                chart1.ChartAreas[0].AxisX.Maximum = 255;
                chart1.ChartAreas[0].AxisX.Title = "Gri Seviye";
                chart1.ChartAreas[0].AxisY.Title = "Piksel Sayısı";

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
                    case "K-Means RGB":
                        KMeansRGB();
                        break;
                    case "K-Means Mahalanobis":
                        KMeansMahalanobis();
                        break;
                    case "Sobel Kenar Bulma":
                        SobelKenarBulma();
                        break;
                    case "Histogram":
                        HistogramCizme();
                        break;
                    case "K-Means Mahalanobis ND":
                        KMeansMahalanobisND();
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
