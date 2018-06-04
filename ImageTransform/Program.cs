using System;
using System.IO;

namespace ImageTransform
{
    internal class Program
    {
        private const string ErrUsage = "Usage: ImageTransform.exe <folderPath> <hexColor> OR [<oldDim> <newDim>]";
        private const string ErrArgCount = "Invalid argument count.";
        private const string ErrInvalidPath = "Invalid folder path.";
        private const string ErrInvalidArg = "Invalid argument.";

        private static void Main(string[] args)
        {
            try
            {
                if (args.Length < 2 || args.Length > 3) throw new ArgumentException(ErrArgCount);
                if (!Directory.Exists(args[0])) throw new ArgumentException(ErrInvalidPath);

                if (args[1].Contains("#"))
                {
                    MakeTransparent(args[0], args[1]);
                }
                else if (args[1].Contains("x"))
                {
                    ResizeImages(args[0], args[1], args[2]);
                }
                else
                {
                    throw new ArgumentException(ErrInvalidArg);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ErrUsage);
            }

            Console.Read();
        }

        private static void MakeTransparent(string folderPath, string hexColor)
        {
            var im = new ImageManipulation {WorkingPath = folderPath};
            im.MadeTransparent += (s, e) => Console.WriteLine(e.OldImageName + " --> " + e.NewImageName);
            im.MakeTransparent(hexColor);
        }

        private static void ResizeImages(string folderPath, string oldDim, string newDim)
        {
            var im = new ImageManipulation {WorkingPath = folderPath};
            im.ImageResized += (s, e) => Console.WriteLine(e.ImageName + ": " + e.OldDims + " --> " + e.NewDims);
            im.Resize(oldDim, newDim);
        }
    }
}
