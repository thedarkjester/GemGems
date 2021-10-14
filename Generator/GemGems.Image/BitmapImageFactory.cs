using GemGems.App.Generator;
using GemGems.App.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GemGems.Image
{
    public class BitmapImageFactory : IImageFactory
    {
        public async Task<byte[]> CombineAsync(IEnumerable<Layer> layers)
        {
            var images = new List<Bitmap>();
            Bitmap baseImage = null;

            try
            {
                await Task.WhenAll(layers.Select(x => x.File));

                var (width, height) = GetImageDimensions(layers.First(x => x.Modifications.IsBase).File.Result);

                baseImage = new Bitmap(width, height);

                foreach (var image in layers)
                {
                    var bitmap = new Bitmap(image.File.Result);
                    if (bitmap.Width != width ||
                        bitmap.Height != height)
                    {
                        //bitmap = ScaleImage(bitmap, 2000, 2000);
                    }

                    var iWidth = bitmap.Width;
                    var iHeight = bitmap.Height;

                    var (X, Y, Scale) = image.Modifications.CalculateOffsets(new LayerPosition
                    {
                        BaseX = width,
                        BaseY = height,
                        CurrentX = iWidth,
                        CurrentY = iHeight
                    });

                    //if (counter == 2)
                    //{
                    //    image.SetModifications(width / 2 - iWidth / 2, height / 2 - iHeight / 2);
                    //}

                    //if (counter == 3)
                    //{
                    //    image.SetModifications(width / 2 + previousWidth / 2 - iWidth / 2, height / 2 + previousHeight / 2 - iHeight / 2);
                    //}

                    using var graphics = Graphics.FromImage(baseImage);
                    graphics.DrawImage(
                      Scale.HasValue ? ScaleImageBy(Scale.Value, bitmap) : bitmap,
                      new Rectangle(X, Y, width, height),
                      0,
                      0,
                      width,
                      height,
                      GraphicsUnit.Pixel,
                      null);


                    images.Add(bitmap);
                }

                using var memoryStream = new MemoryStream();
                baseImage.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);

                return memoryStream.ToArray();
            }
            catch (Exception)
            {
                baseImage?.Dispose();

                throw;
            }

            static (int, int) GetImageDimensions(Stream layer)
            {
                var tempBitmap = new Bitmap(layer);
                var width = tempBitmap.Width;
                var height = tempBitmap.Height;

                tempBitmap?.Dispose();

                return (width, height);
            }
        }

        public static System.Drawing.Image DrawText(string text, Font fontOptional = null, Color? textColorOptional = null, Color? backColorOptional = null, Size? minSizeOptional = null)
        {
            var font = new Font("Arial", 16);
            if (fontOptional != null)
                font = fontOptional;

            Color textColor = Color.Black;
            if (textColorOptional != null)
                textColor = (Color)textColorOptional;

            Color backColor = Color.White;
            if (backColorOptional != null)
                backColor = (Color)backColorOptional;

            Size minSize = Size.Empty;
            if (minSizeOptional != null)
                minSize = (Size)minSizeOptional;

            //first, create a dummy bitmap just to get a graphics object
            SizeF textSize;
            using (System.Drawing.Image img = new Bitmap(1, 1))
            {
                using (Graphics drawing = Graphics.FromImage(img))
                {
                    //measure the string to see how big the image needs to be
                    textSize = drawing.MeasureString(text, font);
                    if (!minSize.IsEmpty)
                    {
                        textSize.Width = textSize.Width > minSize.Width ? textSize.Width : minSize.Width;
                        textSize.Height = textSize.Height > minSize.Height ? textSize.Height : minSize.Height;
                    }
                }
            }

            //create a new image of the right size
            System.Drawing.Image retImg = new Bitmap((int)textSize.Width, (int)textSize.Height);
            using (var drawing = Graphics.FromImage(retImg))
            {
                //paint the background
                drawing.Clear(backColor);

                //create a brush for the text
                using (Brush textBrush = new SolidBrush(textColor))
                {
                    drawing.DrawString(text, font, textBrush, 0, 0);
                    drawing.Save();
                }
            }
            return retImg;
        }

        public Bitmap ScaleImageBy(int multiplier, System.Drawing.Image image)
        {
            return ScaleImage(image, image.Width * multiplier, image.Height * multiplier);
        }

        public Bitmap ScaleImage(System.Drawing.Image image, int maxWidth, int maxHeight)
        {
            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var newImage = new Bitmap(maxWidth, maxHeight);
            using (var graphics = Graphics.FromImage(newImage))
            {
                // Calculate x and y which center the image
                int y = (maxHeight / 2) - newHeight / 2;
                int x = (maxWidth / 2) - newWidth / 2;

                // Draw image on x and y with newWidth and newHeight
                graphics.DrawImage(image, x, y, newWidth, newHeight);
            }

            return newImage;
        }
    }
}

