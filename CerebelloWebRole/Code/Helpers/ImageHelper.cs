using System;
using System.Collections.Generic;
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

        public static Image ResizeImage(Image img, int width, int height)
        {
            Bitmap b = new Bitmap(width, height);
            Graphics g = Graphics.FromImage((Image)b);

            g.DrawImage(img, 0, 0, width, height);
            g.Dispose();

            return (Image)b;
        }
    }
}
