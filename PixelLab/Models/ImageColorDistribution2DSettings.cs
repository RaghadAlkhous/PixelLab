using PixelLab.Enums;

namespace PixelLab.Models
{
    public class ImageColorDistribution2DSettings
    {
        public ImageColorProjection2DType ProjectionType { get; set; }
        public int MaxSampleCount { get; set; }
        public int PointSize { get; set; }

        public ImageColorDistribution2DSettings()
        {
            ProjectionType = ImageColorProjection2DType.Rgb_RG;
            MaxSampleCount = 25000;
            PointSize = 1;
        }
    }
}