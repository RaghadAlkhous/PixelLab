using System;
using System.Drawing;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using PixelLab.Models;
using PixelLab.Enums;

namespace PixelLab.Controls
{
    public class ImageColorDistribution3DControl : UserControl
    {
        private readonly GLControl _glControl;

        private ImageColorDistribution3DResult _result;
        private int _selectedPointIndex;

        private float _yaw;
        private float _pitch;
        private float _zoom;

        private bool _isRotating;
        private Point _lastMousePosition;

        private float _pointSize;

        public event EventHandler<ImageColorPoint3D> PointSelected;

        public ImageColorDistribution3DControl()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.Black;

            _selectedPointIndex = -1;
            _yaw = 25.0f;
            _pitch = -20.0f;
            _zoom = 1.6f;
            _pointSize = 2.0f;

            _glControl = new GLControl(new GraphicsMode(32, 24, 0, 4))
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black
            };

            Controls.Add(_glControl);

            _glControl.Load += GlControl_Load;
            _glControl.Paint += GlControl_Paint;
            _glControl.Resize += GlControl_Resize;

            _glControl.MouseDown += GlControl_MouseDown;
            _glControl.MouseMove += GlControl_MouseMove;
            _glControl.MouseUp += GlControl_MouseUp;
            _glControl.MouseWheel += GlControl_MouseWheel;
            _glControl.DoubleClick += GlControl_DoubleClick;
        }

        public void SetDistribution(ImageColorDistribution3DResult result, float pointSize)
        {
            _result = result;
            _selectedPointIndex = -1;
            _pointSize = pointSize < 1.0f ? 1.0f : pointSize;

            _glControl.Invalidate();
        }

        public void ClearDistribution()
        {
            _result = null;
            _selectedPointIndex = -1;
            _glControl.Invalidate();
        }

        public void ResetView()
        {
            _yaw = 25.0f;
            _pitch = -20.0f;
            _zoom = 1.6f;
            _glControl.Invalidate();
        }

        private void GlControl_Load(object sender, EventArgs e)
        {
            _glControl.MakeCurrent();

            GL.ClearColor(Color.FromArgb(18, 18, 18));
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.PointSmooth);
            GL.Hint(HintTarget.PointSmoothHint, HintMode.Nicest);
        }

        private void GlControl_Resize(object sender, EventArgs e)
        {
            if (_glControl.ClientSize.Height == 0)
                return;

            _glControl.MakeCurrent();

            GL.Viewport(0, 0, _glControl.ClientSize.Width, _glControl.ClientSize.Height);

            _glControl.Invalidate();
        }

        private void GlControl_Paint(object sender, PaintEventArgs e)
        {
            if (!_glControl.Context.IsCurrent)
                _glControl.MakeCurrent();

            RenderScene();

            _glControl.SwapBuffers();
        }

        private void RenderScene()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            SetupMatrices();

            DrawReferenceGeometry();

            if (_result != null)
            {
                DrawPoints();
                DrawSelectedPoint();
            }
        }

        private void SetupMatrices()
        {
            int width = Math.Max(1, _glControl.ClientSize.Width);
            int height = Math.Max(1, _glControl.ClientSize.Height);

            float aspect = width / (float)height;

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            if (aspect >= 1.0f)
                GL.Ortho(-_zoom * aspect, _zoom * aspect, -_zoom, _zoom, -10.0, 10.0);
            else
                GL.Ortho(-_zoom, _zoom, -_zoom / aspect, _zoom / aspect, -10.0, 10.0);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            GL.Rotate(_pitch, 1.0f, 0.0f, 0.0f);
            GL.Rotate(_yaw, 0.0f, 1.0f, 0.0f);
        }

        private void DrawAxesAndBox()
        {
            GL.LineWidth(1.0f);

            GL.Begin(PrimitiveType.Lines);

            // X axis
            GL.Color3(Color.Red);
            GL.Vertex3(-1.1f, -1.1f, -1.1f);
            GL.Vertex3(1.1f, -1.1f, -1.1f);

            // Y axis
            GL.Color3(Color.Lime);
            GL.Vertex3(-1.1f, -1.1f, -1.1f);
            GL.Vertex3(-1.1f, 1.1f, -1.1f);

            // Z axis
            GL.Color3(Color.DeepSkyBlue);
            GL.Vertex3(-1.1f, -1.1f, -1.1f);
            GL.Vertex3(-1.1f, -1.1f, 1.1f);

            GL.End();

            using (PenDummy())
            {
                DrawBoxLines();
            }
        }

        private IDisposable PenDummy()
        {
            return new DummyDisposable();
        }

        private class DummyDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }

        private void DrawBoxLines()
        {
            GL.Color3(Color.FromArgb(90, 90, 90));

            float min = -1.0f;
            float max = 1.0f;

            GL.Begin(PrimitiveType.Lines);

            // bottom square
            GL.Vertex3(min, min, min); GL.Vertex3(max, min, min);
            GL.Vertex3(max, min, min); GL.Vertex3(max, max, min);
            GL.Vertex3(max, max, min); GL.Vertex3(min, max, min);
            GL.Vertex3(min, max, min); GL.Vertex3(min, min, min);

            // top square
            GL.Vertex3(min, min, max); GL.Vertex3(max, min, max);
            GL.Vertex3(max, min, max); GL.Vertex3(max, max, max);
            GL.Vertex3(max, max, max); GL.Vertex3(min, max, max);
            GL.Vertex3(min, max, max); GL.Vertex3(min, min, max);

            // verticals
            GL.Vertex3(min, min, min); GL.Vertex3(min, min, max);
            GL.Vertex3(max, min, min); GL.Vertex3(max, min, max);
            GL.Vertex3(max, max, min); GL.Vertex3(max, max, max);
            GL.Vertex3(min, max, min); GL.Vertex3(min, max, max);

            GL.End();
        }

        private void DrawPoints()
        {
            if (_result == null)
                return;

            GL.PointSize(_pointSize);
            GL.Begin(PrimitiveType.Points);

            for (int i = 0; i < _result.Points.Count; i++)
            {
                if (i == _selectedPointIndex)
                    continue;

                ImageColorPoint3D point = _result.Points[i];

                GL.Color3(point.DisplayColor);
                GL.Vertex3(point.X, point.Y, point.Z);
            }

            GL.End();
        }

        private void DrawSelectedPoint()
        {
            if (_result == null)
                return;

            if (_selectedPointIndex < 0 || _selectedPointIndex >= _result.Points.Count)
                return;

            ImageColorPoint3D point = _result.Points[_selectedPointIndex];

            GL.PointSize(_pointSize + 6.0f);
            GL.Begin(PrimitiveType.Points);

            GL.Color3(Color.White);
            GL.Vertex3(point.X, point.Y, point.Z);

            GL.End();

            GL.PointSize(_pointSize + 3.0f);
            GL.Begin(PrimitiveType.Points);

            GL.Color3(point.DisplayColor);
            GL.Vertex3(point.X, point.Y, point.Z);

            GL.End();
        }

        private void GlControl_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isRotating = true;
                _lastMousePosition = e.Location;
            }

            if (e.Button == MouseButtons.Right)
            {
                PickNearestPoint(e.Location);
            }
        }

        private void GlControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isRotating)
                return;

            int dx = e.X - _lastMousePosition.X;
            int dy = e.Y - _lastMousePosition.Y;

            _yaw += dx * 0.5f;
            _pitch += dy * 0.5f;

            _lastMousePosition = e.Location;

            _glControl.Invalidate();
        }

        private void GlControl_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                _isRotating = false;
        }

        private void GlControl_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0)
                _zoom *= 0.9f;
            else
                _zoom *= 1.1f;

            if (_zoom < 0.35f)
                _zoom = 0.35f;

            if (_zoom > 5.0f)
                _zoom = 5.0f;

            _glControl.Invalidate();
        }

        private void GlControl_DoubleClick(object sender, EventArgs e)
        {
            MouseEventArgs mouseEvent = e as MouseEventArgs;

            if (mouseEvent != null)
                PickNearestPoint(mouseEvent.Location);
        }

        private void PickNearestPoint(Point mouse)
        {
            if (_result == null || _result.Points.Count == 0)
                return;

            int bestIndex = -1;
            double bestDistance = double.MaxValue;

            for (int i = 0; i < _result.Points.Count; i++)
            {
                ImageColorPoint3D point = _result.Points[i];

                PointF screen = ProjectPointToScreen(point);

                double dx = screen.X - mouse.X;
                double dy = screen.Y - mouse.Y;

                double distance = Math.Sqrt(dx * dx + dy * dy);

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestIndex = i;
                }
            }

            if (bestIndex >= 0 && bestDistance <= 18.0)
            {
                _selectedPointIndex = bestIndex;

                ImageColorPoint3D selected = _result.Points[bestIndex];

                if (PointSelected != null)
                    PointSelected(this, selected);

                _glControl.Invalidate();
            }
        }

        private PointF ProjectPointToScreen(ImageColorPoint3D point)
        {
            Vector3 rotated = RotatePoint(
                new Vector3(point.X, point.Y, point.Z));

            int width = Math.Max(1, _glControl.ClientSize.Width);
            int height = Math.Max(1, _glControl.ClientSize.Height);

            float aspect = width / (float)height;

            float visibleX;
            float visibleY;

            if (aspect >= 1.0f)
            {
                visibleX = _zoom * aspect;
                visibleY = _zoom;
            }
            else
            {
                visibleX = _zoom;
                visibleY = _zoom / aspect;
            }

            float normalizedX = rotated.X / visibleX;
            float normalizedY = rotated.Y / visibleY;

            float screenX = (normalizedX + 1.0f) * 0.5f * width;
            float screenY = (1.0f - (normalizedY + 1.0f) * 0.5f) * height;

            return new PointF(screenX, screenY);
        }

        private Vector3 RotatePoint(Vector3 p)
        {
            Matrix4 pitchMatrix =
                Matrix4.CreateRotationX(DegreesToRadians(_pitch));

            Matrix4 yawMatrix =
                Matrix4.CreateRotationY(DegreesToRadians(_yaw));

            Vector4 v = new Vector4(p.X, p.Y, p.Z, 1.0f);

            v = Vector4.Transform(v, pitchMatrix);
            v = Vector4.Transform(v, yawMatrix);

            return new Vector3(v.X, v.Y, v.Z);
        }

        private float DegreesToRadians(float degrees)
        {
            return (float)(degrees * Math.PI / 180.0);
        }

        private void DrawReferenceGeometry()
        {
            if (_result == null)
            {
                DrawRgbCubeReference();
                return;
            }

            switch (_result.ProjectionType)
            {
                case ImageColorProjection3DType.RgbCube:
                    DrawRgbCubeReference();
                    break;

                case ImageColorProjection3DType.HsvCylinder:
                    DrawHsvCylinderReference();
                    break;

                case ImageColorProjection3DType.LabSpace:
                    DrawLabReference();
                    break;

                case ImageColorProjection3DType.YCbCrSpace:
                    DrawLumaChromaReference("YCbCr");
                    break;

                case ImageColorProjection3DType.YuvSpace:
                    DrawLumaChromaReference("YUV");
                    break;

                case ImageColorProjection3DType.CmykCmkSpace:
                    DrawCmykSubspaceReference();
                    break;

                default:
                    DrawRgbCubeReference();
                    break;
            }
        }

        private void DrawRgbCubeReference()
        {
            DrawColoredAxes(
                "R",
                "G",
                "B",
                Color.Red,
                Color.Lime,
                Color.DeepSkyBlue);

            DrawWireCube(
                -1.0f,
                1.0f,
                Color.FromArgb(90, 90, 90));
        }

        private void DrawHsvCylinderReference()
        {
            DrawColoredAxes(
                "Hue-X",
                "Hue-Y",
                "Value",
                Color.Red,
                Color.Lime,
                Color.White);

            GL.LineWidth(1.0f);

            Color gridColor = Color.FromArgb(95, 95, 95);

            // Rings at different Value levels
            DrawCircleRing(-1.0f, 1.0f, gridColor);
            DrawCircleRing(0.0f, 1.0f, Color.FromArgb(70, 70, 70));
            DrawCircleRing(1.0f, 1.0f, gridColor);

            // Smaller saturation rings
            DrawCircleRing(-1.0f, 0.5f, Color.FromArgb(55, 55, 55));
            DrawCircleRing(0.0f, 0.5f, Color.FromArgb(45, 45, 45));
            DrawCircleRing(1.0f, 0.5f, Color.FromArgb(55, 55, 55));

            // Vertical lines around cylinder
            int segments = 24;

            GL.Begin(PrimitiveType.Lines);
            GL.Color3(gridColor);

            for (int i = 0; i < segments; i++)
            {
                double angle = i * 2.0 * Math.PI / segments;

                float x = (float)Math.Cos(angle);
                float y = (float)Math.Sin(angle);

                GL.Vertex3(x, y, -1.0f);
                GL.Vertex3(x, y, 1.0f);
            }

            GL.End();

            // Hue spokes at middle level
            GL.Begin(PrimitiveType.Lines);

            for (int i = 0; i < segments; i += 2)
            {
                double angle = i * 2.0 * Math.PI / segments;

                float x = (float)Math.Cos(angle);
                float y = (float)Math.Sin(angle);

                GL.Vertex3(0.0f, 0.0f, 0.0f);
                GL.Vertex3(x, y, 0.0f);
            }

            GL.End();
        }

        private void DrawCircleRing(float z, float radius, Color color)
        {
            int segments = 96;

            GL.Color3(color);
            GL.Begin(PrimitiveType.LineLoop);

            for (int i = 0; i < segments; i++)
            {
                double angle = i * 2.0 * Math.PI / segments;

                float x = (float)(Math.Cos(angle) * radius);
                float y = (float)(Math.Sin(angle) * radius);

                GL.Vertex3(x, y, z);
            }

            GL.End();
        }

        private void DrawLabReference()
        {
            // a axis: green ↔ red
            // b axis: blue ↔ yellow
            // L axis: dark ↔ light

            DrawColoredAxes(
                "a",
                "b",
                "L",
                Color.Red,
                Color.Gold,
                Color.White);

            // Main bounding guide
            DrawWireCube(
                -1.0f,
                1.0f,
                Color.FromArgb(70, 70, 70));

            // Neutral gray L axis at a=0, b=0
            GL.LineWidth(2.0f);
            GL.Begin(PrimitiveType.Lines);

            GL.Color3(Color.White);
            GL.Vertex3(0.0f, 0.0f, -1.0f);
            GL.Vertex3(0.0f, 0.0f, 1.0f);

            GL.End();

            // a-b planes at dark, middle, light L values
            DrawABPlane(-1.0f, Color.FromArgb(45, 45, 45));
            DrawABPlane(0.0f, Color.FromArgb(75, 75, 75));
            DrawABPlane(1.0f, Color.FromArgb(45, 45, 45));

            // Cross axes in the middle a-b plane
            GL.LineWidth(1.5f);
            GL.Begin(PrimitiveType.Lines);

            // a axis
            GL.Color3(Color.Red);
            GL.Vertex3(-1.0f, 0.0f, 0.0f);
            GL.Vertex3(1.0f, 0.0f, 0.0f);

            // b axis
            GL.Color3(Color.Gold);
            GL.Vertex3(0.0f, -1.0f, 0.0f);
            GL.Vertex3(0.0f, 1.0f, 0.0f);

            GL.End();
        }

        private void DrawABPlane(float z, Color color)
        {
            GL.Color3(color);
            GL.LineWidth(1.0f);

            GL.Begin(PrimitiveType.LineLoop);

            GL.Vertex3(-1.0f, -1.0f, z);
            GL.Vertex3(1.0f, -1.0f, z);
            GL.Vertex3(1.0f, 1.0f, z);
            GL.Vertex3(-1.0f, 1.0f, z);

            GL.End();

            GL.Begin(PrimitiveType.Lines);

            GL.Vertex3(-1.0f, 0.0f, z);
            GL.Vertex3(1.0f, 0.0f, z);

            GL.Vertex3(0.0f, -1.0f, z);
            GL.Vertex3(0.0f, 1.0f, z);

            GL.End();
        }

        private void DrawLumaChromaReference(string label)
        {
            DrawColoredAxes(
                "Chroma-X",
                "Chroma-Y",
                "Y",
                Color.DeepSkyBlue,
                Color.OrangeRed,
                Color.White);

            // Bounding guide still useful, but not as "RGB cube"
            DrawWireCube(
                -1.0f,
                1.0f,
                Color.FromArgb(55, 55, 55));

            // Chroma neutral vertical axis: Cb/Cr or U/V = 128
            GL.LineWidth(2.0f);
            GL.Begin(PrimitiveType.Lines);

            GL.Color3(Color.White);
            GL.Vertex3(0.0f, 0.0f, -1.0f);
            GL.Vertex3(0.0f, 0.0f, 1.0f);

            GL.End();

            // Chroma planes at low/mid/high luma
            DrawChromaPlane(-1.0f, Color.FromArgb(45, 45, 45));
            DrawChromaPlane(0.0f, Color.FromArgb(85, 85, 85));
            DrawChromaPlane(1.0f, Color.FromArgb(45, 45, 45));

            // central neutral point line markers
            DrawNeutralChromaMarkers();
        }

        private void DrawChromaPlane(float z, Color color)
        {
            GL.Color3(color);
            GL.LineWidth(1.0f);

            GL.Begin(PrimitiveType.LineLoop);

            GL.Vertex3(-1.0f, -1.0f, z);
            GL.Vertex3(1.0f, -1.0f, z);
            GL.Vertex3(1.0f, 1.0f, z);
            GL.Vertex3(-1.0f, 1.0f, z);

            GL.End();

            GL.Begin(PrimitiveType.Lines);

            GL.Vertex3(-1.0f, 0.0f, z);
            GL.Vertex3(1.0f, 0.0f, z);

            GL.Vertex3(0.0f, -1.0f, z);
            GL.Vertex3(0.0f, 1.0f, z);

            GL.End();
        }

        private void DrawNeutralChromaMarkers()
        {
            GL.PointSize(6.0f);

            GL.Begin(PrimitiveType.Points);

            GL.Color3(Color.White);
            GL.Vertex3(0.0f, 0.0f, -1.0f);
            GL.Vertex3(0.0f, 0.0f, 0.0f);
            GL.Vertex3(0.0f, 0.0f, 1.0f);

            GL.End();
        }

        private void DrawCmykSubspaceReference()
        {
            DrawColoredAxes(
                "C",
                "M",
                "K",
                Color.Cyan,
                Color.Magenta,
                Color.Black);

            // CMYK is 4D. This cube is a selected 3D subspace: C-M-K.
            DrawWireCube(
                -1.0f,
                1.0f,
                Color.FromArgb(80, 80, 80));

            // K axis emphasized because K is black ink / darkness
            GL.LineWidth(2.5f);

            GL.Begin(PrimitiveType.Lines);

            GL.Color3(Color.White);
            GL.Vertex3(-1.0f, -1.0f, -1.0f);
            GL.Color3(Color.Black);
            GL.Vertex3(-1.0f, -1.0f, 1.0f);

            GL.End();

            // C-M planes for low/mid/high K
            DrawCmyPlaneAtK(-1.0f, Color.FromArgb(45, 45, 45));
            DrawCmyPlaneAtK(0.0f, Color.FromArgb(70, 70, 70));
            DrawCmyPlaneAtK(1.0f, Color.FromArgb(45, 45, 45));
        }

        private void DrawCmyPlaneAtK(float z, Color color)
        {
            GL.Color3(color);
            GL.LineWidth(1.0f);

            GL.Begin(PrimitiveType.LineLoop);

            GL.Vertex3(-1.0f, -1.0f, z);
            GL.Vertex3(1.0f, -1.0f, z);
            GL.Vertex3(1.0f, 1.0f, z);
            GL.Vertex3(-1.0f, 1.0f, z);

            GL.End();
        }

        private void DrawWireCube(float min, float max, Color color)
        {
            GL.Color3(color);
            GL.LineWidth(1.0f);

            GL.Begin(PrimitiveType.Lines);

            // bottom square
            GL.Vertex3(min, min, min); GL.Vertex3(max, min, min);
            GL.Vertex3(max, min, min); GL.Vertex3(max, max, min);
            GL.Vertex3(max, max, min); GL.Vertex3(min, max, min);
            GL.Vertex3(min, max, min); GL.Vertex3(min, min, min);

            // top square
            GL.Vertex3(min, min, max); GL.Vertex3(max, min, max);
            GL.Vertex3(max, min, max); GL.Vertex3(max, max, max);
            GL.Vertex3(max, max, max); GL.Vertex3(min, max, max);
            GL.Vertex3(min, max, max); GL.Vertex3(min, min, max);

            // verticals
            GL.Vertex3(min, min, min); GL.Vertex3(min, min, max);
            GL.Vertex3(max, min, min); GL.Vertex3(max, min, max);
            GL.Vertex3(max, max, min); GL.Vertex3(max, max, max);
            GL.Vertex3(min, max, min); GL.Vertex3(min, max, max);

            GL.End();
        }

        private void DrawColoredAxes(
            string xLabel,
            string yLabel,
            string zLabel,
            Color xColor,
            Color yColor,
            Color zColor)
        {
            GL.LineWidth(2.0f);

            GL.Begin(PrimitiveType.Lines);

            // X axis
            GL.Color3(xColor);
            GL.Vertex3(-1.2f, -1.2f, -1.2f);
            GL.Vertex3(1.2f, -1.2f, -1.2f);

            // Y axis
            GL.Color3(yColor);
            GL.Vertex3(-1.2f, -1.2f, -1.2f);
            GL.Vertex3(-1.2f, 1.2f, -1.2f);

            // Z axis
            GL.Color3(zColor);
            GL.Vertex3(-1.2f, -1.2f, -1.2f);
            GL.Vertex3(-1.2f, -1.2f, 1.2f);

            GL.End();
        }
    }
}