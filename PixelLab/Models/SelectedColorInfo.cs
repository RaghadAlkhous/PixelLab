using System.Drawing;

namespace PixelLab.Models
{
    public class SelectedColorInfo
    {
        public Color RgbColor { get; set; }

        public double H { get; set; }
        public double S { get; set; }
        public double V { get; set; }

        public double L { get; set; }
        public double A { get; set; }
        public double LabB { get; set; }

        public double YCbCrY { get; set; }
        public double Cb { get; set; }
        public double Cr { get; set; }

        public double YuvY { get; set; }
        public double U { get; set; }
        public double YuvV { get; set; }

        public double C { get; set; }
        public double M { get; set; }
        public double CmyY { get; set; }
        public double K { get; set; }

        public string SourceDescription { get; set; }

        public SelectedColorInfo()
        {
            RgbColor = Color.Black;
            SourceDescription = "";
        }
    }
}