using System.Collections.Generic;
using PixelLab.Enums;

namespace PixelLab.Models
{
    public class ImageColorDistribution3DResult
    {
        public List<ImageColorPoint3D> Points { get; private set; }

        public ImageColorProjection3DType ProjectionType { get; set; }

        public string Title { get; set; }
        public string XAxisLabel { get; set; }
        public string YAxisLabel { get; set; }
        public string ZAxisLabel { get; set; }

        public int OriginalPixelCount { get; set; }
        public int SampledPointCount { get; set; }
        public long ProcessingMilliseconds { get; set; }

        public ImageColorDistribution3DResult()
        {
            Points = new List<ImageColorPoint3D>();

            Title = "";
            XAxisLabel = "";
            YAxisLabel = "";
            ZAxisLabel = "";

            ProjectionType = ImageColorProjection3DType.RgbCube;
        }
    }
}