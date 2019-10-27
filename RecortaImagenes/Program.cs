using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace RecortaImagenes
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 3 || args.Length > 4)
            {
                Console.WriteLine("RecortaImagenes.exe #ROWS #COLUMNS <InputPathImage> <Optional OutputPathFolder>");
                Console.WriteLine("Ex: RecortaImagenes.exe 3 3  \"C:\\whateverpicture.jgp\" \"F:\\InstagramFeedPics\"");
                Environment.Exit(-1);
            }
            string path;
            uint.TryParse(args[0], out uint Filas);
            uint.TryParse(args[1], out uint Columnas);

            //OutputPath
            if (string.IsNullOrEmpty(args[2]))
            {
                //Path
                path = args[2];
                if (!Directory.Exists(path))
                {
                    Console.WriteLine("This executable has the following structure:");
                    Console.WriteLine("RecortaImagenes.exe #ROWS #COLUMNS <InputPathImage> <Optional OutputPathFolder>");
                    Console.WriteLine("Ex: RecortaImagenes.exe 3 3  \"C:\\whateverpicture.jgp\" \"F:\\InstagramFeedPics\"");
                    Console.WriteLine($"Optional OutputPathFolder was given but it doesn't exists {path}");
                    Environment.Exit(0);
                }

            }
            else
            {
                path = $"{Directory.GetCurrentDirectory()}\\InstagramFeed";
            }


            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            //ClearFiles if Exists
            var AllFiles = Directory.GetFiles(path);
            if (AllFiles.Length > 0)
            {
                foreach (var cf in AllFiles)
                {
                    try
                    {
                        File.Delete(cf);

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                }
                Console.WriteLine($"Path cleared '{path}'");
            }


            //if exist files delete all


            //loadbitmap
            Bitmap myBmp = new Bitmap(args[2]);

            //divide pixels between 
            int Height = (int)Math.Floor(myBmp.Size.Height / (decimal)Filas);
            int width = (int)Math.Floor(myBmp.Size.Width / (decimal)Columnas);

            uint k = Filas * Columnas;

            for (int j = 0; j < Filas; j++)
            {
                for (int i = 0; i < Columnas; i++)
                {
                    Rectangle currentRectangle = new Rectangle(i * width, j * Height, width, Height);
                    Console.WriteLine($"Creating Image {k}\t dimensions--->\tx:{currentRectangle.X}\ty:{currentRectangle.Y}\twidth:{currentRectangle.Width}\theight:{currentRectangle.Height}");
                    var bm = myBmp.Clone(currentRectangle, PixelFormat.Format32bppArgb);
                    bm.Save($"{path}\\{k}.jpg");
                    k--;
                }
            }
        }
    }
}
