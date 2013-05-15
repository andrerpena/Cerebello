using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;

namespace CerebelloWebRole.Code
{
    public class ImageHelper
    {
        public static Image EnsureMaximumDimensions(Image image, int maxWidth, int maxHeight)
        {
            // Prevent using images internal thumbnail
            image.RotateFlip(System.Drawing.RotateFlipType.Rotate180FlipNone);
            image.RotateFlip(System.Drawing.RotateFlipType.Rotate180FlipNone);

            var aspectRatio = ((double)image.Width) / image.Height; //AR = L/A;

            int imageWidth = image.Width;
            int imageHeight = image.Height;

            if (imageWidth > maxWidth)
            {
                imageWidth = maxWidth;
                imageHeight = (int)(imageWidth / aspectRatio);
            }
            if (imageHeight > maxHeight)
            {
                imageHeight = maxHeight;
                imageWidth = (int)(imageHeight * aspectRatio);
            }

            if (image.Width != imageWidth || image.Height != imageHeight)
                return image.GetThumbnailImage(imageWidth, imageHeight, null, IntPtr.Zero);

            return image;
        }

        public static MemoryStream ResizeImage(Stream srcStream, int width, int height, ImageFormat format, bool keepAspect = false, bool canGrow = true)
        {
            var stream = new MemoryStream();
            using (var srcImage = Image.FromStream(srcStream))
            using (var newImage = ResizeImage(srcImage, width, height, keepAspect, canGrow))
            {
                if (newImage == null)
                    return null;

                newImage.Save(stream, format);
                return stream;
            }
        }

        public static Bitmap ResizeImage(Image srcImage, int width, int height, bool keepAspect = false, bool canGrow = true)
        {
            int w2 = width;
            int h2 = height;

            if (keepAspect)
            {
                w2 = Math.Min(w2, (int)(srcImage.Width * height / (float)srcImage.Height));
                h2 = Math.Min(h2, (int)(srcImage.Height * width / (float)srcImage.Width));
            }

            if (!canGrow)
            {
                w2 = Math.Min(w2, srcImage.Width);
                h2 = Math.Min(h2, srcImage.Height);
            }

            if (srcImage.Width == w2 && srcImage.Height == h2)
                return null;

            var newImage = new Bitmap(w2, h2, srcImage.PixelFormat);
            using (var graphics = Graphics.FromImage(newImage))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.DrawImage(srcImage, new Rectangle(0, 0, w2, h2));
                return newImage;
            }
        }
    }
}
