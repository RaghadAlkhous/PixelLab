using System.Drawing;

namespace PixelLab.Models
{
    public struct ImageColorPoint3D
    {
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Z { get; private set; }

        public Color DisplayColor { get; private set; }
        public string CoordinateText { get; private set; }

        public ImageColorPoint3D(float x, float y, float z, Color displayColor, string coordinateText)
        {
            X = x;
            Y = y;
            Z = z;
            DisplayColor = displayColor;
            CoordinateText = coordinateText;
        }
    }
}