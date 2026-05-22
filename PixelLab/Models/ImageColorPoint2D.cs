using System.Drawing;

namespace PixelLab.Models
{
    public struct ImageColorPoint2D
    {
        public float XNormalized { get; private set; }
        public float YNormalized { get; private set; }
        public Color DisplayColor { get; private set; }

        public ImageColorPoint2D(
            float xNormalized,
            float yNormalized,
            Color displayColor)
        {
            XNormalized = xNormalized;
            YNormalized = yNormalized;
            DisplayColor = displayColor;
        }
    }
}