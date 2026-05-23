using System;
using System.Drawing;
using System.Windows.Forms;
using PixelLab.Models;
using PixelLab.Enums;

namespace PixelLab.Controls
{
    public class ImageColorDistribution2DPanelControl : UserControl
    {
        private readonly ComboBox _projectionComboBox;
        private readonly ComboBox _sampleCountComboBox;
        private readonly TrackBar _pointSizeTrackBar;
        private readonly Label _pointSizeLabel;
        private readonly Button _btnRefresh;
        private readonly ImageColorDistribution2DCanvasControl _canvas;

        public event EventHandler RefreshRequested;

        public ImageColorDistribution2DPanelControl()
        {
            Dock = DockStyle.Top;
            Height = 530;
            BackColor = Color.FromArgb(30, 30, 30);

            var title = new Label
            {
                Text = "2D Image Color Distribution",
                Dock = DockStyle.Top,
                Height = 30,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };

            var projectionLabel = CreateLabel("Projection:");
            _projectionComboBox = CreateComboBox();

            AddProjectionItems();

            var sampleLabel = CreateLabel("Max Sample Count:");
            _sampleCountComboBox = CreateComboBox();

            _sampleCountComboBox.Items.Add("5,000");
            _sampleCountComboBox.Items.Add("10,000");
            _sampleCountComboBox.Items.Add("25,000");
            _sampleCountComboBox.Items.Add("50,000");
            _sampleCountComboBox.Items.Add("100,000");
            _sampleCountComboBox.Items.Add("Full Image.!");
            _sampleCountComboBox.SelectedIndex = 2;

            _pointSizeLabel = CreateLabel("Point Size: 1");

            _pointSizeTrackBar = new TrackBar
            {
                Dock = DockStyle.Top,
                Minimum = 1,
                Maximum = 5,
                Value = 1,
                TickFrequency = 1,
                Height = 40
            };

            _btnRefresh = new Button
            {
                Text = "Refresh Distribution",
                Dock = DockStyle.Top,
                Height = 32,
                BackColor = Color.FromArgb(63, 63, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9)
            };

            _canvas = new ImageColorDistribution2DCanvasControl
            {
                Dock = DockStyle.Top,
                Height = 310
            };

            Controls.Add(_canvas);
            Controls.Add(_btnRefresh);
            Controls.Add(_pointSizeTrackBar);
            Controls.Add(_pointSizeLabel);
            Controls.Add(_sampleCountComboBox);
            Controls.Add(sampleLabel);
            Controls.Add(_projectionComboBox);
            Controls.Add(projectionLabel);
            Controls.Add(title);

            _pointSizeTrackBar.Scroll += delegate
            {
                _pointSizeLabel.Text =
                    "Point Size: " + _pointSizeTrackBar.Value;
            };

            _btnRefresh.Click += delegate
            {
                if (RefreshRequested != null)
                    RefreshRequested(this, EventArgs.Empty);
            };

            _projectionComboBox.SelectedIndex = 0;
        }

        private Label CreateLabel(string text)
        {
            return new Label
            {
                Text = text,
                Dock = DockStyle.Top,
                Height = 22,
                ForeColor = Color.Gainsboro,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
        }

        private ComboBox CreateComboBox()
        {
            return new ComboBox
            {
                Dock = DockStyle.Top,
                Height = 28,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9)
            };
        }

        private void AddProjectionItems()
        {
            _projectionComboBox.Items.Add("RGB: R-G");
            _projectionComboBox.Items.Add("RGB: R-B");
            _projectionComboBox.Items.Add("RGB: G-B");

            _projectionComboBox.Items.Add("HSV: H-S");
            _projectionComboBox.Items.Add("HSV: H-V");
            _projectionComboBox.Items.Add("HSV: S-V");

            _projectionComboBox.Items.Add("LAB: a-b");
            _projectionComboBox.Items.Add("LAB: L-a");
            _projectionComboBox.Items.Add("LAB: L-b");

            _projectionComboBox.Items.Add("YCbCr: Cb-Cr");
            _projectionComboBox.Items.Add("YCbCr: Y-Cb");
            _projectionComboBox.Items.Add("YCbCr: Y-Cr");

            _projectionComboBox.Items.Add("YUV: U-V");
            _projectionComboBox.Items.Add("YUV: Y-U");
            _projectionComboBox.Items.Add("YUV: Y-V");

            _projectionComboBox.Items.Add("CMYK: C-M");
            _projectionComboBox.Items.Add("CMYK: C-K");
            _projectionComboBox.Items.Add("CMYK: Y-K");
        }

        public ImageColorDistribution2DSettings GetSettings()
        {
            return new ImageColorDistribution2DSettings
            {
                ProjectionType = SelectedProjection,
                MaxSampleCount = SelectedMaxSampleCount,
                PointSize = _pointSizeTrackBar.Value
            };
        }

        public void SetDistribution(ImageColorDistribution2DResult result)
        {
            if (result == null)
                return;

            _canvas.SetDistribution(result, _pointSizeTrackBar.Value);
        }

        public void ClearDistribution()
        {
            _canvas.ClearDistribution();
        }

        public void SetPanelEnabled(bool enabled)
        {
            _projectionComboBox.Enabled = enabled;
            _sampleCountComboBox.Enabled = enabled;
            _pointSizeTrackBar.Enabled = enabled;
            _btnRefresh.Enabled = enabled;
        }

        private ImageColorProjection2DType SelectedProjection
        {
            get
            {
                switch (_projectionComboBox.SelectedIndex)
                {
                    case 0:
                        return ImageColorProjection2DType.Rgb_RG;
                    case 1:
                        return ImageColorProjection2DType.Rgb_RB;
                    case 2:
                        return ImageColorProjection2DType.Rgb_GB;

                    case 3:
                        return ImageColorProjection2DType.Hsv_HS;
                    case 4:
                        return ImageColorProjection2DType.Hsv_HV;
                    case 5:
                        return ImageColorProjection2DType.Hsv_SV;

                    case 6:
                        return ImageColorProjection2DType.Lab_AB;
                    case 7:
                        return ImageColorProjection2DType.Lab_LA;
                    case 8:
                        return ImageColorProjection2DType.Lab_LB;

                    case 9:
                        return ImageColorProjection2DType.YCbCr_CbCr;
                    case 10:
                        return ImageColorProjection2DType.YCbCr_YCb;
                    case 11:
                        return ImageColorProjection2DType.YCbCr_YCr;

                    case 12:
                        return ImageColorProjection2DType.Yuv_UV;
                    case 13:
                        return ImageColorProjection2DType.Yuv_YU;
                    case 14:
                        return ImageColorProjection2DType.Yuv_YV;

                    case 15:
                        return ImageColorProjection2DType.Cmyk_CM;
                    case 16:
                        return ImageColorProjection2DType.Cmyk_CK;
                    case 17:
                        return ImageColorProjection2DType.Cmyk_YK;

                    default:
                        return ImageColorProjection2DType.Rgb_RG;
                }
            }
        }

        private int SelectedMaxSampleCount
        {
            get
            {
                switch (_sampleCountComboBox.SelectedIndex)
                {
                    case 0:
                        return 5000;
                    case 1:
                        return 10000;
                    case 2:
                        return 25000;
                    case 3:
                        return 50000;
                    case 4:
                        return 100000;
                    case 5:
                        return 0;
                    default:
                        return 25000;
                }
            }
        }
    }
}