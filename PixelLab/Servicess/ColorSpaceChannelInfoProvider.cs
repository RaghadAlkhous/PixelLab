using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PixelLab.Models;

namespace PixelLab.Servicess
{
    public class ColorSpaceChannelInfoProvider
    {
        public static ColorSpaceChannelInfo GetInfo(ColorSpaceType colorSpace)
        {
            switch (colorSpace)
            {
                case ColorSpaceType.RgbOriginal:
                    return new ColorSpaceChannelInfo(
                        colorSpace,
                        "RGB",
                        new string[] { "R", "G", "B" });

                case ColorSpaceType.Grayscale:
                    return new ColorSpaceChannelInfo(
                        colorSpace,
                        "Grayscale",
                        new string[] { "Gray" });

                case ColorSpaceType.Hsv:
                    return new ColorSpaceChannelInfo(
                        colorSpace,
                        "HSV",
                        new string[] { "H", "S", "V" });

                case ColorSpaceType.Lab:
                    return new ColorSpaceChannelInfo(
                        colorSpace,
                        "LAB",
                        new string[] { "L", "a", "b" });

                case ColorSpaceType.YCbCr:
                    // OpenCV / EmguCV يستخدم داخليًا ترتيب YCrCb
                    return new ColorSpaceChannelInfo(
                        colorSpace,
                        "YCbCr / OpenCV YCrCb",
                        new string[] { "Y", "Cr", "Cb" });

                case ColorSpaceType.Yuv:
                    return new ColorSpaceChannelInfo(
                        colorSpace,
                        "YUV",
                        new string[] { "Y", "U", "V" });

                case ColorSpaceType.CmykPreview:
                    return new ColorSpaceChannelInfo(
                        colorSpace,
                        "CMYK",
                        new string[] { "C", "M", "Y", "K" });

                default:
                    throw new NotSupportedException(
                        "Unsupported color space: " + colorSpace);
            }
        }
    }
}
