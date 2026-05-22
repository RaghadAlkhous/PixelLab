using System;
using System.Drawing;
using System.Text;

namespace PixelLab.Utils
{
    public static class ColorValueConversions
    {
        public static void RgbToHsv(
            byte rByte,
            byte gByte,
            byte bByte,
            out double h,
            out double s,
            out double v)
        {
            double r = rByte / 255.0;
            double g = gByte / 255.0;
            double b = bByte / 255.0;

            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));
            double delta = max - min;

            if (delta == 0)
                h = 0;
            else if (max == r)
                h = 60.0 * (((g - b) / delta) % 6.0);
            else if (max == g)
                h = 60.0 * (((b - r) / delta) + 2.0);
            else
                h = 60.0 * (((r - g) / delta) + 4.0);

            if (h < 0)
                h += 360.0;

            s = max == 0 ? 0 : delta / max;
            v = max;
        }

        public static void RgbToYCbCr(
            byte r,
            byte g,
            byte b,
            out double y,
            out double cb,
            out double cr)
        {
            y = 0.299 * r + 0.587 * g + 0.114 * b;
            cb = 128.0 - 0.168736 * r - 0.331264 * g + 0.5 * b;
            cr = 128.0 + 0.5 * r - 0.418688 * g - 0.081312 * b;

            y = Clamp255(y);
            cb = Clamp255(cb);
            cr = Clamp255(cr);
        }

        public static void RgbToYuv(
            byte r,
            byte g,
            byte b,
            out double y,
            out double u,
            out double v)
        {
            y = 0.299 * r + 0.587 * g + 0.114 * b;

            double rawU = -0.14713 * r - 0.28886 * g + 0.436 * b;
            double rawV = 0.615 * r - 0.51499 * g - 0.10001 * b;

            u = Clamp255(rawU + 128.0);
            v = Clamp255(rawV + 128.0);
            y = Clamp255(y);
        }

        public static void RgbToCmyk(
            byte rByte,
            byte gByte,
            byte bByte,
            out double c,
            out double m,
            out double y,
            out double k)
        {
            double r = rByte / 255.0;
            double g = gByte / 255.0;
            double b = bByte / 255.0;

            k = 1.0 - Math.Max(r, Math.Max(g, b));

            c = 0.0;
            m = 0.0;
            y = 0.0;

            if (k < 1.0)
            {
                c = (1.0 - r - k) / (1.0 - k);
                m = (1.0 - g - k) / (1.0 - k);
                y = (1.0 - b - k) / (1.0 - k);
            }

            c = Clamp01(c);
            m = Clamp01(m);
            y = Clamp01(y);
            k = Clamp01(k);
        }

        public static void RgbToLab(
            byte rByte,
            byte gByte,
            byte bByte,
            out double l,
            out double a,
            out double b)
        {
            double r = PivotRgb(rByte / 255.0);
            double g = PivotRgb(gByte / 255.0);
            double bb = PivotRgb(bByte / 255.0);

            double x = r * 0.4124564 + g * 0.3575761 + bb * 0.1804375;
            double y = r * 0.2126729 + g * 0.7151522 + bb * 0.0721750;
            double z = r * 0.0193339 + g * 0.1191920 + bb * 0.9503041;

            double xRef = 0.95047;
            double yRef = 1.00000;
            double zRef = 1.08883;

            double fx = PivotXyz(x / xRef);
            double fy = PivotXyz(y / yRef);
            double fz = PivotXyz(z / zRef);

            l = 116.0 * fy - 16.0;
            a = 500.0 * (fx - fy);
            b = 200.0 * (fy - fz);
        }

        public static string BuildFullColorDescription(Color color)
        {
            double h, s, v;
            double l, a, labB;
            double ycbcrY, cb, cr;
            double yuvY, u, vv;
            double c, m, yy, k;

            RgbToHsv(color.R, color.G, color.B, out h, out s, out v);
            RgbToLab(color.R, color.G, color.B, out l, out a, out labB);
            RgbToYCbCr(color.R, color.G, color.B, out ycbcrY, out cb, out cr);
            RgbToYuv(color.R, color.G, color.B, out yuvY, out u, out vv);
            RgbToCmyk(color.R, color.G, color.B, out c, out m, out yy, out k);

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("RGB");
            sb.AppendLine("R = " + color.R + ", G = " + color.G + ", B = " + color.B);
            sb.AppendLine();

            sb.AppendLine("HSV");
            sb.AppendLine(
                "H = " + h.ToString("0.0") + "°" +
                ", S = " + (s * 100.0).ToString("0.0") + "%" +
                ", V = " + (v * 100.0).ToString("0.0") + "%");
            sb.AppendLine();

            sb.AppendLine("LAB");
            sb.AppendLine(
                "L = " + l.ToString("0.0") +
                ", a = " + a.ToString("0.0") +
                ", b = " + labB.ToString("0.0"));
            sb.AppendLine();

            sb.AppendLine("YCbCr");
            sb.AppendLine(
                "Y = " + ycbcrY.ToString("0.0") +
                ", Cb = " + cb.ToString("0.0") +
                ", Cr = " + cr.ToString("0.0"));
            sb.AppendLine();

            sb.AppendLine("YUV");
            sb.AppendLine(
                "Y = " + yuvY.ToString("0.0") +
                ", U = " + u.ToString("0.0") +
                ", V = " + vv.ToString("0.0"));
            sb.AppendLine();

            sb.AppendLine("CMYK Preview");
            sb.AppendLine(
                "C = " + (c * 100.0).ToString("0.0") + "%" +
                ", M = " + (m * 100.0).ToString("0.0") + "%" +
                ", Y = " + (yy * 100.0).ToString("0.0") + "%" +
                ", K = " + (k * 100.0).ToString("0.0") + "%");

            return sb.ToString();
        }

        private static double PivotRgb(double value)
        {
            if (value > 0.04045)
                return Math.Pow((value + 0.055) / 1.055, 2.4);

            return value / 12.92;
        }

        private static double PivotXyz(double value)
        {
            if (value > 0.008856)
                return Math.Pow(value, 1.0 / 3.0);

            return 7.787 * value + 16.0 / 116.0;
        }

        private static double Clamp01(double value)
        {
            if (value < 0.0)
                return 0.0;

            if (value > 1.0)
                return 1.0;

            return value;
        }

        private static double Clamp255(double value)
        {
            if (value < 0.0)
                return 0.0;

            if (value > 255.0)
                return 255.0;

            return value;
        }
    }
}
