using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;

namespace WpFVersion
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow
    {
        private const double Columnas = 3.0;


        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Pbar.Value = e.ProgressPercentage;
        }
        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            for (int i = 0; i < 100; i++)
            {
                (sender as BackgroundWorker)?.ReportProgress(i);
                Thread.Sleep(100);
            }
        }

        private BitmapImage _currentImage = new BitmapImage();
        static string _mes = DateTime.Now.ToString("MMMM", CultureInfo.InvariantCulture);
        static string _dia = DateTime.Now.Day.ToString();
        private string _path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + $"\\{_mes}{_dia}\\";

        void saveCroppedBitmap(CroppedBitmap image, string path, uint k)
        {
            string local = $"{path}\\{k}.jpg";

            using (FileStream mStream = new FileStream(local, FileMode.Create))
            {
                JpegBitmapEncoder jEncoder = new JpegBitmapEncoder();
                jEncoder.Frames.Add(BitmapFrame.Create(image));
                jEncoder.Save(mStream);
            }
        }





        public MainWindow()
        {
            InitializeComponent();
            TbTarget.Text = _path;
        }

        private void BtExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Grid_Drop(object sender, DragEventArgs e)
        {
            if (null != e.Data && e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var data = e.Data.GetData(DataFormats.FileDrop) as string[];

                var list = (data ?? throw new InvalidOperationException()).ToList();
                foreach (var bm in list)
                {
                    Uri uri = new Uri(bm, UriKind.Absolute);
                    _currentImage = new BitmapImage(uri);
                    Img.Source = new BitmapImage(uri);
                    LbResult.Content = $"Loaded: {_currentImage.Width}x{_currentImage.Height}";
                }
            }
            else
            {
                LbResult.Content = "Problem Loading Image";
            }

        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            TbTarget.Text = _path;
        }

        private void BtRun_Click(object sender, RoutedEventArgs e)
        {

            if (_currentImage.UriSource == null)
            {
                LbResult.Content = "Drag an Image to above section";
                return;
            }


            if (string.IsNullOrEmpty(TbTarget.Text))
            {
                //Path
                if (!Directory.Exists(_path))
                {
                    var dialog = new System.Windows.Forms.FolderBrowserDialog();
                    System.Windows.Forms.DialogResult result = dialog.ShowDialog();

                    if (!string.IsNullOrWhiteSpace(dialog.SelectedPath) && result == System.Windows.Forms.DialogResult.OK && Directory.Exists(dialog.SelectedPath))
                    {
                        TbTarget.Text = dialog.SelectedPath;
                    }
                    else
                    {
                        LbResult.Content = "Problem, Target Folder Doesn't exists";
                    }
                }

            }

            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += worker_DoWork;
            worker.ProgressChanged += worker_ProgressChanged;

            //get path
            _path = $"{TbTarget.Text}";

            //if doesn't exist create it 
            if (!Directory.Exists(_path))
            {
                Directory.CreateDirectory(_path);
            }

            //Cleaning
            var allFiles = Directory.GetFiles(_path);
            if (allFiles.Length > 0)
            {
                foreach (var cf in allFiles)
                {
                    try
                    {
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        File.Delete(cf);

                    }
                    catch (Exception ex)
                    {
                        LbResult.Content = $"Error: {ex.Message}";
                    }
                }
                LbResult.Content = $"Path cleared '{_path}'";
            }


            //divide pixels between 
            int width = (int)Math.Floor(_currentImage.PixelWidth / Columnas);
            int height = width;

            int filas = (int)Math.Floor(_currentImage.PixelHeight / (double)height);


            uint k = (uint)(Columnas * filas);

            for (var j = 0; j < filas; j++)
            {
                for (var i = 0; i < (int)Columnas; i++)
                {
                    try
                    {
                        Int32Rect rect = new Int32Rect(i * width, j * height, width, height);
                        CroppedBitmap segment = new CroppedBitmap(_currentImage, rect);
                        saveCroppedBitmap(segment, _path, k);
                        k--;
                    }
                    catch (Exception ex)
                    {
                        LbResult.Content = ex.Message;
                    }

                }
            }


        }


        private void BtnChange_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            if (!string.IsNullOrWhiteSpace(dialog.SelectedPath) && result == System.Windows.Forms.DialogResult.OK && Directory.Exists(dialog.SelectedPath))
            {
                TbTarget.Text = dialog.SelectedPath;
            }
            else
            {
                LbResult.Content = "Problem, Target Folder Doesn't exists";
            }
        }
    }
}
