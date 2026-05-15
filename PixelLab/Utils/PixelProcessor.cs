using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace PixelLab.Utils
{

    public static class PixelProcessor
    {
        public static Bitmap ProcessWithLockBits(Bitmap source, Func<Color, Color> pixelFunction)
        {
            if (source == null || pixelFunction == null)
                return null;
            Bitmap result = new Bitmap(source.Width, source.Height, PixelFormat.Format24bppRgb);

            Rectangle rect = new Rectangle(0, 0, source.Width, source.Height);

            BitmapData sourceData = source.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            BitmapData resultData = result.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            int bytes = Math.Abs(sourceData.Stride) * source.Height;

            byte[] sourceBytes = new byte[bytes];
            byte[] resultBytes = new byte[bytes];

            System.Runtime.InteropServices.Marshal.Copy(sourceData.Scan0, sourceBytes, 0, bytes);

            for (int y = 0; y < source.Height; y++)
            {
                for (int x = 0; x < source.Width; x++)
                {
                    
                    int index = y * Math.Abs(sourceData.Stride) + x * 3;

                    byte b = sourceBytes[index];       // Blue component
                    byte g = sourceBytes[index + 1];   // Green component    
                    byte r = sourceBytes[index + 2];   // Red component

                    Color originalColor = Color.FromArgb(r, g, b);

                    Color modifiedColor = pixelFunction(originalColor);

                    resultBytes[index] = modifiedColor.B;
                    resultBytes[index + 1] = modifiedColor.G;
                    resultBytes[index + 2] = modifiedColor.R;
                }
            }

            System.Runtime.InteropServices.Marshal.Copy(resultBytes, 0, resultData.Scan0, bytes);


            source.UnlockBits(sourceData);
            result.UnlockBits(resultData);

            return result;
        }
    }
}