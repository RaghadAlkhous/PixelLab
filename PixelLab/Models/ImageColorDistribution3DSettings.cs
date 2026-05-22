using System;
using PixelLab.Enums;

namespace PixelLab.Models
{
    public class ImageColorDistribution3DSettings
    {
        public ImageColorProjection3DType ProjectionType { get; set; }
        public int MaxSampleCount { get; set; }
        public float PointSize { get; set; }

        public ImageColorDistribution3DSettings()
        {
            ProjectionType = ImageColorProjection3DType.RgbCube;
            MaxSampleCount = 25000;
            PointSize = 2.0f;
        }
    }
}
