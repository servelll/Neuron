using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace NeuroNet
{
    public partial class MainForm : Form
    {
        private NeuroWeb nw;
        private Point startP;
        private int[,] arr;

        //переменные обучающей выборки
        private string trainpixelFile = @"MNIST/train-images.idx3-ubyte";
        private string trainlabelFile = @"MNIST/train-labels.idx1-ubyte";
        private string testpixelFile = @"MNIST/t10k-images.idx3-ubyte";
        private string testlabelFile = @"MNIST/t10k-labels.idx1-ubyte";
        private DigitImage[] temp_images_mas = null;

        //переменная для режима редактирования
        private bool is_changed;

        //инициализация
        public MainForm()
        {
            InitializeComponent();
            is_changed = false;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            Clear();
            nw = new NeuroWeb();
            StatusUpdate();
        }
        private void StatusUpdate()
        {
            string[] items = nw.GetLiteras();
            if (items.Length > 0)
            {
                comboBox.Items.Clear();
                comboBox.Items.AddRange(items);
                comboBox.SelectedIndex = 0;
            }
            label1.Text = "Состояние нейросети: " + " элементов -  " + items.Count();
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("Сохранить результаты?", "", MessageBoxButtons.YesNo) == DialogResult.Yes) nw.SaveState();
        }

        //рисование
        private void PictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Point endP = new Point(e.X, e.Y);
                Bitmap image = (Bitmap)pictureBox1.Image;
                using (Graphics g = Graphics.FromImage(image))
                {
                    g.DrawLine(new Pen(Color.BlueViolet), startP, endP);
                }
                pictureBox1.Image = image;
                startP = endP;
                is_changed = true;
            }
        }    
        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {    
            startP = new Point(e.X, e.Y);
        }
        private void PictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (is_changed)
                {
                    if (comboBox.Text.Length > 0)
                    {
                        if (nw.GetLiteras().Count() > 0)
                        {
                            Act();
                        }
                        else
                        {
                            DialogResult askLearn = MessageBox.Show("Обучить нейросеть на этом примере?", "", MessageBoxButtons.YesNo);
                            if (askLearn == DialogResult.No) return;
                            nw.SetTraining(comboBox.Text, arr, (double)trackBar1.Value/1000f);
                            StatusUpdate();
                            Clear();
                        }
                    }
                    else
                    {
                        MessageBox.Show("В нейросети нет элементов, и поэтому мы можем ее только обучить\n Подпишите картинку в поле справа");
                    }
                }
                else
                {
                    //проверка на первый прогон
                    if (nw.GetLiteras().Count() == 0)
                    {
                        MessageBox.Show("В нейросети нет элементов, надо ее сначала обучить, для этого:\n 1) Нарисуйте в панели необходимое, и подпишите ее справа\n ИЛИ\n 2) Загрузите обучающую выборку");
                    }
                    else
                    {
                        Act();
                    }
                }
            }
        }
        
        private void DrawFromComboBoxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clear();
            pictureBox1.Image = NeiroGraphUtils.DrawLitera(pictureBox1.Image, comboBox.Text);
        }

        private void Act()
        {
            int[,] clipArr = NeiroGraphUtils.CutImageToArray((Bitmap)pictureBox1.Image, new Point(pictureBox1.Width, pictureBox1.Height));
            if (clipArr == null) return;
            arr = NeiroGraphUtils.LeadArray(clipArr, new int[NeuroWeb.neironInArrayWidth, NeuroWeb.neironInArrayHeight]);
            pictureBox2.Image = NeiroGraphUtils.GetBitmapFromArr(clipArr);
            pictureBox3.Image = NeiroGraphUtils.GetBitmapFromArr(arr);
            string s = nw.CheckLitera(arr);
            if (s == null) s = "null";
            DialogResult askResult;
            if (режимПереобученияToolStripMenuItem.Checked)
            {
                askResult = MessageBox.Show("Результат = " + s + " ?", "", MessageBoxButtons.YesNo);
            }
            else
            {
                askResult = MessageBox.Show("Результат = " + s);
            }
            if (askResult == DialogResult.Yes || !режимПереобученияToolStripMenuItem.Checked) return;
            DialogResult askLearn = MessageBox.Show("Переобучить нейросеть на этом примере?", "", MessageBoxButtons.YesNo);
            if (askLearn == DialogResult.No) return;
            string s2 = Microsoft.VisualBasic.Interaction.InputBox("Введите верное значение");
            nw.SetTraining(s2, arr, (double)trackBar1.Value / 1000f);
            StatusUpdate();
            Clear();
        }

        //очистка
        private void Clear()
        {
            NeiroGraphUtils.ClearImage(pictureBox1);
            NeiroGraphUtils.ClearImage(pictureBox2);
            NeiroGraphUtils.ClearImage(pictureBox3);
            is_changed = false;
        }
        private void Clear_button_Click(object sender, EventArgs e)
        {
            Clear();
        }
        private void ClearWebToolStripMenuItem_Click(object sender, EventArgs e)
        {
            nw.ClearWeb();
            StatusUpdate();
        }

        //загрузка выборки
        public static int ReverseBytes(int v)
        {
            byte[] intAsBytes = BitConverter.GetBytes(v);
            Array.Reverse(intAsBytes);
            return BitConverter.ToInt32(intAsBytes, 0);
        }
        public static DigitImage[] LoadData(string pixelFile, string labelFile)
        {
            FileStream ifsPixels = new FileStream(pixelFile, FileMode.Open);
            FileStream ifsLabels = new FileStream(labelFile, FileMode.Open);
            BinaryReader brImages = new BinaryReader(ifsPixels);
            BinaryReader brLabels = new BinaryReader(ifsLabels);

            int numImages = (int)brImages.BaseStream.Length / (28 * 28);           
            DigitImage[] result = new DigitImage[numImages];
            int[,] pixels = new int[28, 28];

            int magic1 = brImages.ReadInt32(); // обратный порядок байтов
            magic1 = ReverseBytes(magic1); // преобразуем в формат Intel
            int imageCount = brImages.ReadInt32();
            imageCount = ReverseBytes(imageCount);
            int numRows = brImages.ReadInt32();
            numRows = ReverseBytes(numRows);
            int numCols = brImages.ReadInt32();
            numCols = ReverseBytes(numCols);
            int magic2 = brLabels.ReadInt32();
            magic2 = ReverseBytes(magic2);
            int numLabels = brLabels.ReadInt32();
            numLabels = ReverseBytes(numLabels);
            for (int di = 0; di < numImages; ++di)
            {
                for (int i = 0; i < 28; ++i) // получаем пиксельные значения 28х28
                {
                    for (int j = 0; j < 28; ++j)
                    {
                        byte b = brImages.ReadByte();
                        if (b > 0)
                        {
                            pixels[i, j] = 1;
                        } else
                        {
                            pixels[i, j] = 0;
                        }
                    }
                }
                int lbl = brLabels.ReadByte(); // получаем маркеры
                DigitImage dImage = new DigitImage(28, 28, pixels, lbl);
                result[di] = dImage;
            } // по каждому изображению
            ifsPixels.Close(); brImages.Close();
            ifsLabels.Close(); brLabels.Close();
            return result;
        }
        private void LoadTestImagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            temp_images_mas = LoadData(testpixelFile, testlabelFile);
            double count = 0;
            foreach (DigitImage item in temp_images_mas)
            {
                int[,] arr = NeiroGraphUtils.LeadArray(item.pixels, new int[NeuroWeb.neironInArrayWidth, NeuroWeb.neironInArrayHeight]);
                if (nw.CheckLitera(arr) == Convert.ToString(item.label)) count++;
            }
            MessageBox.Show("Коэффициент распознавания тестовой выборки (" + temp_images_mas.Count() + ") = " + 100*count/temp_images_mas.Count() + "%");
        }
        private void LoadTrainToolStripMenuItem_Click(object sender, EventArgs e)
        {
            temp_images_mas = LoadData(trainpixelFile, trainlabelFile);
            int[] stat = new int[10];
            foreach (DigitImage item in temp_images_mas)
            {
                stat[item.label]++;
                nw.SetTraining(item.label.ToString(), item.pixels, (double)trackBar1.Value / 1000f);
            }
            string s = "";
            int all = 0;
            for (int i = 0; i < 10; i++)
            {
                s += i + ": " + stat[i] + "\n";
                all += stat[i];
            }
            s += "Всего было добавлено: " + all;
            StatusUpdate();
            MessageBox.Show("Обучение было произведено успешно!\n" + s);
            (sender as ToolStripMenuItem).Enabled = false;
        }

        private void РежимПереобученияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            режимПереобученияToolStripMenuItem.Checked = !режимПереобученияToolStripMenuItem.Checked;
        }

        private void TrackBar1_Scroll(object sender, EventArgs e)
        {
            label5.Text = ((double)(sender as TrackBar).Value / 1000f).ToString();
        }
    }
}
