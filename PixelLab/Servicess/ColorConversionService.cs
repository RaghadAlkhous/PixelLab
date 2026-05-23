using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace PixelLab.Servicess
{
    public static class ColorConversionService
    {
        #region RGB
        public static Bitmap ConvertToRGB(Bitmap source)
        {
            if (source == null) return null;
            return new Bitmap(source);
        }

        #endregion

        #region CMY 
        public static Bitmap ConvertToCMY(Bitmap source)
        {
            if (source == null) return null;

            Bitmap normalized = null;
            Bitmap result = null;

            try
            {
                // التأكد من أن الصورة 24-bit
                normalized = EnsurePixelFormat24bpp(source);

                result = new Bitmap(normalized.Width, normalized.Height, PixelFormat.Format24bppRgb);
                Rectangle rect = new Rectangle(0, 0, result.Width, result.Height);

                BitmapData srcData = normalized.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                BitmapData resData = result.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

                try
                {
                    int bytes = Math.Abs(srcData.Stride) * normalized.Height;
                    int stride = Math.Abs(srcData.Stride);

                    byte[] srcBytes = new byte[bytes];
                    byte[] resBytes = new byte[bytes];

                    Marshal.Copy(srcData.Scan0, srcBytes, 0, bytes);

                    for (int y = 0; y < normalized.Height; y++)
                    {
                        for (int x = 0; x < normalized.Width; x++)
                        {
                            int index = y * stride + x * 3;

                            if (index + 2 < bytes)
                            {   
                                byte b = srcBytes[index];
                                byte g = srcBytes[index + 1];
                                byte r = srcBytes[index + 2];

                                resBytes[index] = (byte)(255 - b);     // Yellow
                                resBytes[index + 1] = (byte)(255 - g); // Magenta
                                resBytes[index + 2] = (byte)(255 - r); // Cyan
                            }
                        }
                    }

                    Marshal.Copy(resBytes, 0, resData.Scan0, bytes);
                }
                finally
                {
                    normalized.UnlockBits(srcData);
                    result.UnlockBits(resData);
                }

                return result;
            }
            catch (Exception ex)
            {
                normalized?.Dispose();
                result?.Dispose();
                throw new InvalidOperationException($"فشل التحويل إلى CMY: {ex.Message}", ex);
            }
        }
        public static Bitmap ConvertCMYToRGBForDisplay(Bitmap cmyImage)
        {
            if (cmyImage == null) return null;

            Bitmap normalized = null;
            Bitmap result = null;

            try
            {
                normalized = EnsurePixelFormat24bpp(cmyImage);
                result = new Bitmap(normalized.Width, normalized.Height, PixelFormat.Format24bppRgb);
                Rectangle rect = new Rectangle(0, 0, result.Width, result.Height);

                BitmapData srcData = normalized.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                BitmapData resData = result.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

                try
                {
                    int bytes = Math.Abs(srcData.Stride) * normalized.Height;
                    int stride = Math.Abs(srcData.Stride);
                    byte[] srcBytes = new byte[bytes];
                    byte[] resBytes = new byte[bytes];

                    Marshal.Copy(srcData.Scan0, srcBytes, 0, bytes);

                    for (int y = 0; y < normalized.Height; y++)
                    {
                        for (int x = 0; x < normalized.Width; x++)
                        {
                            int index = y * stride + x * 3;

                            if (index + 2 < bytes)
                            {
                                resBytes[index]     = (byte)(255 - srcBytes[index]);
                                resBytes[index + 1] = (byte)(255 - srcBytes[index + 1]);
                                resBytes[index + 2] = (byte)(255 - srcBytes[index + 2]);
                            }
                        }
                    }

                    Marshal.Copy(resBytes, 0, resData.Scan0, bytes);
                }
                finally
                {
                    normalized.UnlockBits(srcData);
                    result.UnlockBits(resData);
                }

                return result;
            }
            catch (Exception ex)
            {
                normalized?.Dispose();
                result?.Dispose();
                throw new InvalidOperationException($"فشل التحويل من CMY: {ex.Message}", ex);
            }
        }
        #endregion

        #region HSV, YUV, YCbCr, LAB 

        public static Bitmap ConvertToHSV(Bitmap source)
        {
            return ConvertUsingOpenCV(source, ColorConversion.Bgr2Hsv);
        }

        public static Bitmap ConvertHSVToRGBForDisplay(Bitmap hsvImage)
        {
            return ConvertBackToRGB(hsvImage, ColorConversion.Hsv2Bgr);
        }

        public static Bitmap ConvertToYUV(Bitmap source)
        {
            return ConvertUsingOpenCV(source, ColorConversion.Bgr2Yuv);
        }

        public static Bitmap ConvertYUVToRGBForDisplay(Bitmap yuvImage)
        {
            return ConvertBackToRGB(yuvImage, ColorConversion.Yuv2Bgr);
        }

        public static Bitmap ConvertToYCbCr(Bitmap source)
        {
            return ConvertUsingOpenCV(source, ColorConversion.Bgr2YCrCb);
        }

        public static Bitmap ConvertYCbCrToRGBForDisplay(Bitmap ycbcrImage)
        {
            return ConvertBackToRGB(ycbcrImage, ColorConversion.YCrCb2Bgr);
        }

        public static Bitmap ConvertToLAB(Bitmap source)
        {
            return ConvertUsingOpenCV(source, ColorConversion.Bgr2Lab);
        }

        public static Bitmap ConvertLABToRGBForDisplay(Bitmap labImage)
        {
            return ConvertBackToRGB(labImage, ColorConversion.Lab2Bgr);
        }

        #endregion

        #region Grayscale
        public static Bitmap ConvertToGrayscale(Bitmap source)
        {
            if (source == null) return null;

            using (Mat srcMat = source.ToMat())
            using (Mat bgrMat = new Mat())
            using (Mat grayMat = new Mat())
            {
                CvInvoke.CvtColor(srcMat, bgrMat, ColorConversion.Rgb2Bgr);
                CvInvoke.CvtColor(bgrMat, grayMat, ColorConversion.Bgr2Gray);
                return grayMat.ToBitmap();
            }
        }

        public static Bitmap ConvertToBinary(Bitmap source, int threshold = 127)
        {
            if (source == null) return null;

            using (Bitmap gray = ConvertToGrayscale(source))
            using (Mat grayMat = gray.ToMat())
            using (Mat binaryMat = new Mat())
            {
                CvInvoke.Threshold(grayMat, binaryMat, threshold, 255, ThresholdType.Binary);
                return binaryMat.ToBitmap();
            }
        }

        #endregion

        #region Display Helper
        public static Bitmap ConvertForDisplay(string colorSpace, Bitmap image)
        {
            if (image == null) return null;

            string space = colorSpace.ToUpper();

            switch (space)
            {
                case "CMY":
                    return ConvertCMYToRGBForDisplay(image);
                case "HSV":
                    return ConvertHSVToRGBForDisplay(image);
                case "YUV":
                    return ConvertYUVToRGBForDisplay(image);
                case "YCBCR":
                    return ConvertYCbCrToRGBForDisplay(image);
                case "LAB":
                    return ConvertLABToRGBForDisplay(image);
                case "RGB":
                default:
                    return new Bitmap(image);
            }
        }

        #endregion

        #region 
        private static Bitmap ConvertUsingOpenCV(Bitmap source, ColorConversion conversion)
        {
            if (source == null) return null;

            try
            {
                using (Mat srcMat = source.ToMat())
                using (Mat bgrMat = new Mat())
                using (Mat dstMat = new Mat())
                {
                    CvInvoke.CvtColor(srcMat, bgrMat, ColorConversion.Rgb2Bgr);
                    CvInvoke.CvtColor(bgrMat, dstMat, conversion);
                    return dstMat.ToBitmap();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw new InvalidOperationException($"فشل التحويل إلى {conversion}", ex);
            }
        }

        private static Bitmap ConvertBackToRGB(Bitmap source, ColorConversion reverseConversion)
        {
            if (source == null) return null;

            try
            {
                using (Mat srcMat = source.ToMat())
                using (Mat bgrMat = new Mat())
                using (Mat rgbMat = new Mat())
                {
                    CvInvoke.CvtColor(srcMat, bgrMat, reverseConversion);
                    CvInvoke.CvtColor(bgrMat, rgbMat, ColorConversion.Bgr2Rgb);
                    return rgbMat.ToBitmap();
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"فشل التحويل العكسي من {reverseConversion}", ex);
            }
        }

        private static Bitmap EnsurePixelFormat24bpp(Bitmap source)
        {
            if (source == null) return null;

            if (source.PixelFormat == PixelFormat.Format24bppRgb)
                return new Bitmap(source);

            Bitmap converted = new Bitmap(source.Width, source.Height, PixelFormat.Format24bppRgb);

            using (Graphics g = Graphics.FromImage(converted))
            {
                g.Clear(Color.White); 
                g.DrawImageUnscaled(source, 0, 0);
            }

            return converted;
        }

        #endregion
    }
}