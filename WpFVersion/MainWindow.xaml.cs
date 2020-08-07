using System;
using System.ComponentModel;
using System.Diagnostics;
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

        private const double _columnas = 3.0;
        private int _filas;

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
            TbRows.Text = "3";
            TbTarget.Text = _path;
        }

        private void BtExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private async void Grid_Drop(object sender, DragEventArgs e)
        {
            if (null != e.Data && e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var data = e.Data.GetData(DataFormats.FileDrop) as string[];

                var list = (data ?? throw new InvalidOperationException()).ToList();
                foreach (var bm in list)
                {
                    Uri uri = new Uri(bm, UriKind.Absolute);
                    _currentImage = new BitmapImage(uri);
                    Img.Source =  new BitmapImage(uri);
                    LbResult.Content = $"Loaded: {_currentImage.PixelWidth}x{_currentImage.PixelHeight}";
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


            if (string.IsNullOrEmpty(TbTarget.Text) && string.IsNullOrWhiteSpace(TbRows.Text) && Convert.ToInt32(TbRows.Text) >= 3)
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

            int width = (int)Math.Floor(_currentImage.PixelWidth / _columnas);
            int height = width;
            

            if (_currentImage.PixelWidth == _currentImage.PixelHeight)
            {
                //It's a square
                _filas = (int)Math.Floor(_currentImage.PixelHeight / (double)height);
                uint k = (uint)(_columnas * _filas);

                for (var j = 0; j < _filas; j++)
                {
                    for (var i = 0; i < (int)_columnas; i++)
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
            else if(_currentImage.PixelWidth > _currentImage.PixelHeight)
            {
                int.TryParse(TbRows.Text, out _filas);
                int candidateHeight = (int)Math.Floor(_currentImage.PixelHeight / (double)_filas);
                width = candidateHeight;

                int MitadDeLaFoto= (int)Math.Floor(_currentImage.PixelWidth / 2.0);
                int Medio = MitadDeLaFoto - (int)(width * 1.5);

                //landscape
                uint k = (uint)(_columnas * _filas);

                for (var j = 0; j < _filas; j++)
                {
                    for (var i = 0; i < (int)_columnas; i++)
                    {
                        try
                        {
                            Int32Rect rect = new Int32Rect(Medio + (i * width), j * candidateHeight, width, candidateHeight);
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
            else
            {
                int.TryParse(TbRows.Text, out _filas);
                int candidateHeight = (int)Math.Floor(_currentImage.PixelHeight / (double)_filas);

                if (candidateHeight < width )
                {
                    //use candidateheigh
                    width = candidateHeight;
                    height = candidateHeight;
                }
                //portrait+
                int MitadDeLaFoto = (int)Math.Floor(_currentImage.PixelWidth / 2.0);
                int Start = MitadDeLaFoto - (int) (width * 1.5);

               

                //landscape
                uint k = (uint)(_columnas * _filas);

                for (var j = 0; j < _filas; j++)
                {
                    for (var i = 0; i < (int)_columnas; i++)
                    {
                        try
                        {
                            Int32Rect rect = new Int32Rect(Start + (i * width), j * height, width, height);
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

        private void BtnGo_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(TbTarget.Text) && Directory.Exists(TbTarget.Text))
            {
                Process.Start(TbTarget.Text);
            }
            else
            {
                LbResult.Content = $"The Folder {TbTarget.Text} doesn't exists"; 
            }
        }
    }
}
