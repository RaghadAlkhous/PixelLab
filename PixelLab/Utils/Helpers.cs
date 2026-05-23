using System;
using System.Drawing.Imaging;
using System.Drawing;


namespace PixelLab.Utils
{
    public static class Helpers
    {
        public static Bitmap Ensure24bppRgb(Bitmap source)
        {
            if (source.PixelFormat == PixelFormat.Format24bppRgb)
                return new Bitmap(source);

            Bitmap converted = new Bitmap(
                source.Width,
                source.Height,
                PixelFormat.Format24bppRgb);

            using (Graphics graphics = Graphics.FromImage(converted))
            {
                graphics.DrawImage(source, 0, 0, source.Width, source.Height);
            }

            return converted;
        }

        public static byte ToByte(double value)
        {
            if (value < 0.0)
                return 0;

            if (value > 255.0)
                return 255;

            return (byte)Math.Round(value);
        }
    }
}
