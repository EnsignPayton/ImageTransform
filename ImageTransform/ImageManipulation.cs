using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;

namespace ImageTransform
{
    /// <summary>
    /// Batch manipulates images
    /// </summary>
    internal class ImageManipulation
    {
        public event EventHandler<MadeTransparentEventArgs> MadeTransparent;
        public event EventHandler<ImageResizedEventArgs> ImageResized;

        public string WorkingPath { get; set; }

        /// <summary>
        /// Makes transparent PNGs from all BMP images in <see cref="WorkingPath"/>.
        /// </summary>
        /// <param name="transparentColor">Color to make transparent</param>
        public void MakeTransparent(Color transparentColor)
        {
            foreach (var file in Directory.EnumerateFiles(WorkingPath, "*.bmp"))
            {
                var newName = Path.ChangeExtension(file, "png");
                using (var image = Image.FromFile(file))
                using (var bitmap = new Bitmap(image))
                {
                    bitmap.MakeTransparent(transparentColor);
                    bitmap.Save(newName, ImageFormat.Png);
                    MadeTransparent?.Invoke(this, new MadeTransparentEventArgs(file, newName));
                }
            }
        }

        /// <summary>
        /// Makes transparent PNGs from all BMP images in <see cref="WorkingPath"/>.
        /// </summary>
        /// <param name="hexValue">RGB Hex of color to make transparent</param>
        public void MakeTransparent(string hexValue)
        {
            var transparentColor = Color.FromArgb(int.Parse(hexValue.Replace("#", ""), NumberStyles.HexNumber));

            MakeTransparent(transparentColor);
        }

        /// <summary>
        /// Changes the dimensions of all image in <see cref="WorkingPath"/> with <see cref="oldX"/> x <see cref="oldY"/> to <see cref="newX"/> x <see cref="newY"/>.
        /// </summary>
        /// <param name="oldX">Old width</param>
        /// <param name="oldY">Old height</param>
        /// <param name="newX">New width</param>
        /// <param name="newY">New height</param>
        public void Resize(int oldX, int oldY, int newX, int newY)
        {
            foreach (var file in Directory.EnumerateFiles(WorkingPath, "*.png", SearchOption.AllDirectories))
            {
                string tmpFile = file + ".tmp";
                using (var image = Image.FromFile(file))
                {
                    if (image.Width != oldX || image.Height != oldY) continue;

                    using (var bitmap = ResizeImage(image, newX, newY))
                    {
                        bitmap.Save(tmpFile, ImageFormat.Png);
                        ImageResized?.Invoke(this, new ImageResizedEventArgs(file, oldX, oldY, newX, newY));
                    }
                }

                File.Delete(file);
                File.Move(tmpFile, file);
            }
        }

        /// <summary>
        /// Changes the dimensions of all image in <see cref="WorkingPath"/> with <see cref="oldDim"/> to <see cref="newDim"/>.
        /// </summary>
        /// <param name="oldDim">Old dimensions (XXxYY)</param>
        /// <param name="newDim">New dimensions (XXxYY)</param>
        public void Resize(string oldDim, string newDim)
        {
            var oldDims = oldDim.Split('x');
            var newDims = newDim.Split('x');
            var oldX = int.Parse(oldDims[0]);
            var oldY = int.Parse(oldDims[1]);
            var newX = int.Parse(newDims[0]);
            var newY = int.Parse(newDims[1]);

            Resize(oldX, oldY, newX, newY);
        }

        // Credit where credit is due: https://stackoverflow.com/a/24199315
        private static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(image))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.NearestNeighbor; // Original: HighQualityBicubic
                graphics.SmoothingMode = SmoothingMode.None; // Original: HighQuality
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var imageAttr = new ImageAttributes())
                {
                    imageAttr.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width,
                        image.Height, GraphicsUnit.Pixel, imageAttr);
                }
            }

            return destImage;
        }
    }

    internal class MadeTransparentEventArgs : EventArgs
    {
        public string OldImageName { get; }
        public string NewImageName { get; }

        public MadeTransparentEventArgs(string oldImageName, string newImageName)
        {
            OldImageName = oldImageName;
            NewImageName = newImageName;
        }
    }

    internal class ImageResizedEventArgs : EventArgs
    {
        public string ImageName { get; }
        public int OldX { get; }
        public int OldY { get; }
        public int NewX { get; }
        public int NewY { get; }
        public string OldDims => OldX + "x" + OldY;
        public string NewDims => NewX + "x" + NewY;

        public ImageResizedEventArgs(string imageName, int oldX, int oldY, int newX, int newY)
        {
            ImageName = imageName;
            OldX = oldX;
            OldY = oldY;
            NewX = newX;
            NewY = newY;
        }
    }
}
