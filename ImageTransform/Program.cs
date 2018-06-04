using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;

namespace ImageTransform
{
    internal class Program
    {
        private const string ErrUsage = "Usage: ImageTransform.exe <folderPath> <hexColor>";
        private const string ErrArgCount = "Invalid argument count.";
        private const string ErrInvalidPath = "Invalid folder path.";

        private static void Main(string[] args)
        {
            try
            {
                if (args.Length != 2) throw new ArgumentException(ErrArgCount);
                if (!Directory.Exists(args[0])) throw new ArgumentException(ErrInvalidPath);

                var folderPath = args[0];
                var targetColor = Color.FromArgb(int.Parse(args[1].Replace("#", ""), NumberStyles.HexNumber));

                foreach (var file in Directory.EnumerateFiles(folderPath))
                {
                    if (!Path.GetExtension(file).Contains("bmp")) continue;
                    var newName = Path.ChangeExtension(file, "png");
                    using (var image = Image.FromFile(file))
                    using (var bitmap = new Bitmap(image))
                    {
                        bitmap.MakeTransparent(targetColor);
                        bitmap.Save(newName, ImageFormat.Png);
                        Console.WriteLine(file + " --> " + newName);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ErrUsage);
            }

            Console.Read();
        }
    }
}
