using System;
using System.Drawing;
using System.Windows.Forms;
using PixelLab.Models;

namespace PixelLab.Controls
{
    public class ImageColorDistribution2DCanvasControl : UserControl
    {
        private ImageColorDistribution2DResult _result;
        private int _pointSize;

        public ImageColorDistribution2DCanvasControl()
        {
            DoubleBuffered = true;
            Dock = DockStyle.Fill;
            BackColor = Color.FromArgb(22, 22, 22);
            _pointSize = 1;
        }

        public void SetDistribution(
            ImageColorDistribution2DResult result,
            int pointSize)
        {
            _result = result;
            _pointSize = pointSize < 1 ? 1 : pointSize;

            Invalidate();
        }

        public void ClearDistribution()
        {
            _result = null;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.Clear(Color.FromArgb(22, 22, 22));

            Rectangle plotRect = GetPlotRectangle();

            DrawPlotBackground(e.Graphics, plotRect);

            if (_result == null || _result.Points.Count == 0)
            {
                DrawEmptyMessage(e.Graphics, plotRect);
                return;
            }

            DrawPoints(e.Graphics, plotRect);
            DrawTextInfo(e.Graphics, plotRect);
        }

        private Rectangle GetPlotRectangle()
        {
            int left = 45;
            int top = 35;
            int right = 20;
            int bottom = 45;

            int width = Math.Max(10, Width - left - right);
            int height = Math.Max(10, Height - top - bottom);

            return new Rectangle(left, top, width, height);
        }

        private void DrawPlotBackground(Graphics graphics, Rectangle plotRect)
        {
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(12, 12, 12)))
            {
                graphics.FillRectangle(brush, plotRect);
            }

            using (Pen gridPen = new Pen(Color.FromArgb(45, 45, 45)))
            {
                for (int i = 0; i <= 4; i++)
                {
                    int x = plotRect.Left + i * plotRect.Width / 4;
                    int y = plotRect.Top + i * plotRect.Height / 4;

                    graphics.DrawLine(
                        gridPen,
                        x,
                        plotRect.Top,
                        x,
                        plotRect.Bottom);

                    graphics.DrawLine(
                        gridPen,
                        plotRect.Left,
                        y,
                        plotRect.Right,
                        y);
                }
            }

            using (Pen axisPen = new Pen(Color.FromArgb(180, 180, 180)))
            {
                graphics.DrawRectangle(axisPen, plotRect);
            }
        }

        private void DrawPoints(Graphics graphics, Rectangle plotRect)
        {
            foreach (ImageColorPoint2D point in _result.Points)
            {
                int x = plotRect.Left +
                        (int)(point.XNormalized * plotRect.Width);

                int y = plotRect.Top +
                        (int)(point.YNormalized * plotRect.Height);

                using (SolidBrush brush = new SolidBrush(point.DisplayColor))
                {
                    if (_pointSize <= 1)
                    {
                        graphics.FillRectangle(brush, x, y, 1, 1);
                    }
                    else
                    {
                        graphics.FillEllipse(
                            brush,
                            x - _pointSize / 2,
                            y - _pointSize / 2,
                            _pointSize,
                            _pointSize);
                    }
                }
            }
        }

        private void DrawTextInfo(Graphics graphics, Rectangle plotRect)
        {
            using (Font titleFont = new Font("Segoe UI", 9, FontStyle.Bold))
            using (Font smallFont = new Font("Segoe UI", 8))
            using (SolidBrush textBrush = new SolidBrush(Color.White))
            using (SolidBrush subTextBrush = new SolidBrush(Color.LightGray))
            {
                graphics.DrawString(
                    _result.Title,
                    titleFont,
                    textBrush,
                    plotRect.Left,
                    8);

                graphics.DrawString(
                    _result.XAxisLabel,
                    smallFont,
                    textBrush,
                    plotRect.Left + plotRect.Width / 2 - 10,
                    plotRect.Bottom + 18);

                graphics.DrawString(
                    _result.YAxisLabel,
                    smallFont,
                    textBrush,
                    5,
                    plotRect.Top + plotRect.Height / 2 - 10);

                string info =
                    "Samples: " + _result.SampledPointCount.ToString("N0") +
                    " / Pixels: " + _result.OriginalPixelCount.ToString("N0") +
                    " | " + _result.ProcessingMilliseconds + " ms";

                graphics.DrawString(
                    info,
                    smallFont,
                    subTextBrush,
                    plotRect.Left,
                    plotRect.Bottom + 2);
            }
        }

        private void DrawEmptyMessage(Graphics graphics, Rectangle plotRect)
        {
            using (Font font = new Font("Segoe UI", 10))
            using (SolidBrush brush = new SolidBrush(Color.LightGray))
            {
                string text =
                    "No color distribution generated.\n" +
                    "Load an image and press Refresh Distribution.";

                SizeF size = graphics.MeasureString(text, font);

                graphics.DrawString(
                    text,
                    font,
                    brush,
                    plotRect.Left + (plotRect.Width - size.Width) / 2,
                    plotRect.Top + (plotRect.Height - size.Height) / 2);
            }
        }
    }
}