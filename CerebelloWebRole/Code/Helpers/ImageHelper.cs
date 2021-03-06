﻿using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace CerebelloWebRole.Code
{
    public class ImageHelper
    {
        public static Image EnsureMaximumDimensions(Image image, int maxWidth, int maxHeight)
        {
            // Prevent using images internal thumbnail
            image.RotateFlip(RotateFlipType.Rotate180FlipNone);
            image.RotateFlip(RotateFlipType.Rotate180FlipNone);

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

        /// <summary>
        /// Creates a thumbnail image of a file in the storage.
        /// </summary>
        /// <param name="originalMetadataId">Metadata entry ID for the original image file.</param>
        /// <param name="maxWidth">Maximum width of the thumbnail image.</param>
        /// <param name="maxHeight">Maximum height of the thumbnail image.</param>
        /// <param name="sourceFullStorageFileName">Name of the source image file.</param>
        /// <param name="thumbFullStorageFileName">Name of the thumbnail image cache file.</param>
        /// <param name="loadFromCache">Whether to use a cached thumbnail or not.</param>
        /// <param name="storage">Storage service used to get file data.</param>
        /// <param name="fileMetadataProvider">File metadata provider used to create thumbnail image metadata.</param>
        /// <returns>Returns the result of the thumbnail creation process.</returns>
        public static CreateThumbResult TryGetOrCreateThumb(
            int originalMetadataId,
            int maxWidth,
            int maxHeight,
            string sourceFullStorageFileName,
            string thumbFullStorageFileName,
            bool loadFromCache,
            IBlobStorageManager storage,
            IFileMetadataProvider fileMetadataProvider)
        {
            var srcBlobLocation = new BlobLocation(sourceFullStorageFileName);
            var thumbBlobLocation = new BlobLocation(thumbFullStorageFileName);

            if (loadFromCache && !string.IsNullOrEmpty(thumbFullStorageFileName))
            {
                var thumbStream = storage.DownloadFileFromStorage(thumbBlobLocation);
                if (thumbStream != null)
                {
                    using (thumbStream)
                    using (var stream = new MemoryStream((int)thumbStream.Length))
                    {
                        thumbStream.CopyTo(stream);
                        {
                            return new CreateThumbResult(
                                CreateThumbStatus.Ok,
                                stream.ToArray(),
                                MimeTypesHelper.GetContentType(Path.GetExtension(thumbFullStorageFileName)));
                        }
                    }
                }
            }

            if (!StringHelper.IsImageFileName(sourceFullStorageFileName))
                return new CreateThumbResult(CreateThumbStatus.SourceIsNotImage, null, null);

            var srcStream = storage.DownloadFileFromStorage(srcBlobLocation);

            if (srcStream == null)
                return new CreateThumbResult(CreateThumbStatus.SourceFileNotFound, null, null);

            string contentType;
            byte[] array;
            using (srcStream)
            using (var srcImage = Image.FromStream(srcStream))
            {
                var imageSizeMegabytes = srcImage.Width * srcImage.Height * 4 / 1024000.0;
                if (imageSizeMegabytes > 40.0)
                    return new CreateThumbResult(CreateThumbStatus.SourceImageTooLarge, null, null);

                using (var newImage = ResizeImage(srcImage, maxWidth, maxHeight, keepAspect: true, canGrow: false))
                using (var newStream = new MemoryStream())
                {
                    if (newImage == null)
                    {
                        srcStream.Position = 0;
                        srcStream.CopyTo(newStream);
                        contentType = MimeTypesHelper.GetContentType(Path.GetExtension(sourceFullStorageFileName));
                    }
                    else
                    {
                        var imageFormat = (newImage.Width * newImage.Height > 10000)
                            ? ImageFormat.Jpeg
                            : ImageFormat.Png;

                        contentType = (newImage.Width * newImage.Height > 10000)
                            ? "image/jpeg"
                            : "image/png";

                        newImage.Save(newStream, imageFormat);
                    }

                    array = newStream.ToArray();

                    if (loadFromCache && newImage != null && !string.IsNullOrEmpty(thumbFullStorageFileName))
                    {
                        // saving thumbnail image file metadata
                        var relationType = string.Format("thumb-{0}x{1}", maxWidth, maxHeight);
                        var metadata = fileMetadataProvider.CreateRelated(
                            originalMetadataId,
                            relationType,
                            thumbBlobLocation.ContainerName,
                            thumbBlobLocation.FileName,
                            thumbBlobLocation.BlobName,
                            null);

                        fileMetadataProvider.SaveChanges();

                        storage.UploadFileToStorage(new MemoryStream(array), thumbBlobLocation);
                    }
                }
            }

            return new CreateThumbResult(CreateThumbStatus.Ok, array, contentType);
        }
    }
}
