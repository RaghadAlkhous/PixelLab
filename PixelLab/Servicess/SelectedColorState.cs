using System;
using System.Drawing;
using PixelLab.Models;
using PixelLab.Utils;

namespace PixelLab.Services
{
    public class SelectedColorState
    {
        private SelectedColorInfo _currentColorInfo;

        public event EventHandler<SelectedColorInfo> SelectedColorChanged;

        public SelectedColorInfo CurrentColorInfo
        {
            get { return _currentColorInfo; }
        }

        public SelectedColorState()
        {
            _currentColorInfo = BuildInfo(Color.Black, "Initial color");
        }

        public void SetSelectedColor(Color color, string sourceDescription)
        {
            _currentColorInfo = BuildInfo(color, sourceDescription);

            if (SelectedColorChanged != null)
                SelectedColorChanged(this, _currentColorInfo);
        }

        private SelectedColorInfo BuildInfo(Color color, string sourceDescription)
        {
            double h, s, v;
            double l, a, labB;
            double ycbcrY, cb, cr;
            double yuvY, u, yuvV;
            double c, m, cmyY, k;

            ColorValueConversions.RgbToHsv(
                color.R,
                color.G,
                color.B,
                out h,
                out s,
                out v);

            ColorValueConversions.RgbToLab(
                color.R,
                color.G,
                color.B,
                out l,
                out a,
                out labB);

            ColorValueConversions.RgbToYCbCr(
                color.R,
                color.G,
                color.B,
                out ycbcrY,
                out cb,
                out cr);

            ColorValueConversions.RgbToYuv(
                color.R,
                color.G,
                color.B,
                out yuvY,
                out u,
                out yuvV);

            ColorValueConversions.RgbToCmyk(
                color.R,
                color.G,
                color.B,
                out c,
                out m,
                out cmyY,
                out k);

            return new SelectedColorInfo
            {
                RgbColor = color,

                H = h,
                S = s,
                V = v,

                L = l,
                A = a,
                LabB = labB,

                YCbCrY = ycbcrY,
                Cb = cb,
                Cr = cr,

                YuvY = yuvY,
                U = u,
                YuvV = yuvV,

                C = c,
                M = m,
                CmyY = cmyY,
                K = k,

                SourceDescription = sourceDescription
            };
        }
    }
}