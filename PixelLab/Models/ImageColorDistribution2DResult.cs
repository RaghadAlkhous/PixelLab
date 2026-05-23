using System.Collections.Generic;

namespace PixelLab.Models
{
    public class ImageColorDistribution2DResult
    {
        public List<ImageColorPoint2D> Points { get; private set; }

        public string Title { get; set; }
        public string XAxisLabel { get; set; }
        public string YAxisLabel { get; set; }

        public int OriginalPixelCount { get; set; }
        public int SampledPointCount { get; set; }
        public long ProcessingMilliseconds { get; set; }

        public ImageColorDistribution2DResult()
        {
            Points = new List<ImageColorPoint2D>();

            Title = "";
            XAxisLabel = "";
            YAxisLabel = "";
        }
    }
}