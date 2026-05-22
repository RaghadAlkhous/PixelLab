using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelLab.Enums
{
    public enum ImageColorProjection2DType
    {
        Rgb_RG,
        Rgb_RB,
        Rgb_GB,

        Hsv_HS,
        Hsv_HV,
        Hsv_SV,

        Lab_AB,
        Lab_LA,
        Lab_LB,

        YCbCr_CbCr,
        YCbCr_YCb,
        YCbCr_YCr,

        Yuv_UV,
        Yuv_YU,
        Yuv_YV,

        Cmyk_CM,
        Cmyk_CK,
        Cmyk_YK
    }
}
