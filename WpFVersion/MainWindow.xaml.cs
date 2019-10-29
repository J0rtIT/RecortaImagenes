using System;
using System.ComponentModel;
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
    public partial class MainWindow : Window
    {

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Pbar.Value = e.ProgressPercentage;
        }
        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            for (int i = 0; i < 100; i++)
            {
                (sender as BackgroundWorker).ReportProgress(i);
                Thread.Sleep(100);
            }
        }

        private BitmapImage _currentImage = new BitmapImage();
        private string _path;

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
            TbColumnas.Text = "3";
            TbFilas.Text = "3";
            TbTarget.Text = "C:\\";
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TbFilas.Text = "";
            TbColumnas.Text = "";
            TbTarget.Text = "";
        }



        private void BtRun_Click(object sender, RoutedEventArgs e)
        {
            uint filas = 0;
            uint columnas = 0;

            if (_currentImage.UriSource == null)
            {
                LbResult.Content = "Drag an Image";
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

            //Get numbers
            if (!String.IsNullOrEmpty(TbFilas.Text) || Convert.ToUInt32(TbFilas.Text) > 5)
            {
                uint.TryParse(TbFilas.Text, out filas);
            }
            else
            {
                LbResult.Content = "Rows must be a numeric value and less than 5";
            }

            if (!String.IsNullOrEmpty(TbColumnas.Text) || Convert.ToUInt32(TbColumnas.Text) > 5)
            {
                uint.TryParse(TbColumnas.Text, out columnas);
            }
            else
            {
                LbResult.Content = "Columns must be a numeric value and less than 5";
            }



            //get path
            _path = $"{TbTarget.Text}\\InstagramFeed";

            //if doesnt exist create it 
            if (!Directory.Exists(_path))
            {
                Directory.CreateDirectory(_path);
            }

            //Cleaning
            var AllFiles = Directory.GetFiles(_path);
            if (AllFiles.Length > 0)
            {
                foreach (var cf in AllFiles)
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
            int Height = (int)Math.Floor(_currentImage.PixelHeight / (double)filas);
            int width = (int)Math.Floor(_currentImage.PixelWidth / (double)columnas);

            uint k = filas * columnas;

            for (int j = 0; j < filas; j++)
            {
                for (int i = 0; i < columnas; i++)
                {
                    try
                    {
                        Int32Rect rect = new Int32Rect(i * width, j * Height, width, Height);
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
}
