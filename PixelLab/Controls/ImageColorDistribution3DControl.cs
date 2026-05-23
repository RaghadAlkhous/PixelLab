using System;
using System.Drawing;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using PixelLab.Models;

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

            DrawAxesAndBox();

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
    }
}