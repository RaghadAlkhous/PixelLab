using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace PixelLab.Servicess
{
    public static class ImageInfoService
    {
        public static string GetImageInfo(Bitmap image, string filePath)
        {
            if (image == null)
                return "No image loaded";

            try
            {
                FileInfo fileInfo = new FileInfo(filePath);
                int bitDepth = Image.GetPixelFormatSize(image.PixelFormat);
                long rawSize = (long)image.Width * image.Height * (bitDepth / 8);
                float aspectRatio = (float)image.Width / image.Height;

                string info =
                    "IMAGE INFORMATION\n" +
                    new string('=', 39) + "\n\n" +
                    $"File Name:        {fileInfo.Name}\n" +
                    $"File Path:        {filePath}\n" +
                    $"File Size:        {FormatFileSize(fileInfo.Length)}\n\n" +
                    "DIMENSIONS\n" +
                    new string('-', 39) + "\n" +
                    $"Width:            {image.Width} pixels\n" +
                    $"Height:           {image.Height} pixels\n" +
                    $"Total Pixels:     {image.Width * image.Height:N0}\n" +
                    $"Aspect Ratio:     {aspectRatio:F2} ({SimplifyRatio(image.Width, image.Height)})\n\n" +
                    "COLOR DEPTH\n" +
                    new string('-', 39) + "\n" +
                    $"Bits per Pixel:   {bitDepth}-bit\n" +
                    $"Bytes per Pixel:  {bitDepth / 8}\n" +
                    $"Pixel Format:     {GetPixelFormatName(image.PixelFormat)}\n" +
                    $"Format Code:      {image.PixelFormat}\n\n" +
                    "MEMORY USAGE\n" +
                    new string('-', 39) + "\n" +
                    $"On Disk:          {FormatFileSize(fileInfo.Length)}\n" +
                    $"In Memory:        {FormatFileSize(rawSize)}\n" +
                    $"Compression:      {(fileInfo.Length > 0 ? ((double)rawSize / fileInfo.Length).ToString("F2") + "x" : "N/A")}\n\n" +
                    new string('=', 39);

                return info;
            }
            catch (Exception ex)
            {
                return $"Error reading image information: {ex.Message}";
            }
        }

        private static string GetPixelFormatName(PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.Format1bppIndexed:
                    return "Monochrome (1-bit)";
                case PixelFormat.Format8bppIndexed:
                    return "Indexed Color (8-bit)";
                case PixelFormat.Format24bppRgb:
                    return "True Color RGB (24-bit)";
                case PixelFormat.Format32bppRgb:
                    return "RGB with Alpha (32-bit)";
                case PixelFormat.Format32bppArgb:
                    return "ARGB with Transparency (32-bit)";
                case PixelFormat.Format48bppRgb:
                    return "High Color RGB (48-bit)";
                case PixelFormat.Format64bppArgb:
                    return "High Color ARGB (64-bit)";
                default:
                    return format.ToString();
            }
        }

        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        private static string SimplifyRatio(int width, int height)
        {
            int gcd = GCD(width, height);
            return $"{width / gcd}:{height / gcd}";
        }

        private static int GCD(int a, int b)
        {
            while (b != 0)
            {
                int temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }

        public static long GetFileSizeInKB(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return 0;
            return new FileInfo(filePath).Length / 1024;
        }
    }
}