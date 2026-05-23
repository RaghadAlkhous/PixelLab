using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using PixelLab.Controls;
using PixelLab.Models;
using PixelLab.Servicess;
using PixelLab.Enums;
using PixelLab.Utils;

namespace PixelLab.Forms
{
    public class ImageColorDistribution3DForm : Form
    {
        private readonly PixelLabWorkspace _workspace;
        private readonly ImageColorDistribution3DService _distributionService;

        private readonly ImageColorDistribution3DControl _viewer;

        private readonly ComboBox _projectionComboBox;
        private readonly ComboBox _sampleCountComboBox;
        private readonly TrackBar _pointSizeTrackBar;
        private readonly Label _pointSizeLabel;

        private readonly Button _btnRefresh;
        private readonly Button _btnResetView;

        private readonly Panel _selectedColorBox;
        private readonly TextBox _selectedColorTextBox;
        private readonly Label _statusLabel;

        public ImageColorDistribution3DForm(PixelLabWorkspace workspace)
        {
            if (workspace == null)
                throw new ArgumentNullException(nameof(workspace));

            _workspace = workspace;
            _distributionService = new ImageColorDistribution3DService();

            Text = "3D Image Color Distribution";
            Width = 1100;
            Height = 750;
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(900, 600);
            BackColor = Color.FromArgb(45, 45, 48);

            _viewer = new ImageColorDistribution3DControl();

            _projectionComboBox = CreateComboBox();
            _sampleCountComboBox = CreateComboBox();

            _pointSizeTrackBar = new TrackBar
            {
                Minimum = 1,
                Maximum = 8,
                Value = 2,
                Dock = DockStyle.Top,
                Height = 45,
                TickFrequency = 1
            };

            _pointSizeLabel = CreateLabel("Point Size: 2");

            _btnRefresh = CreateButton("Refresh 3D Distribution");
            _btnResetView = CreateButton("Reset View");

            _selectedColorBox = new Panel
            {
                Dock = DockStyle.Top,
                Height = 45,
                BackColor = Color.Black
            };

            _selectedColorTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White,
                Font = new Font("Consolas", 9),
                BorderStyle = BorderStyle.FixedSingle
            };

            _statusLabel = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 28,
                Text = "Ready",
                ForeColor = Color.White,
                BackColor = Color.FromArgb(0, 122, 204),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0)
            };

            BuildLayout();
            FillControls();
            WireEvents();
        }

        private void BuildLayout()
        {
            var mainSplit = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 760,
                BackColor = Color.FromArgb(45, 45, 48)
            };

            mainSplit.Panel1.Controls.Add(_viewer);

            var rightPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                BackColor = Color.FromArgb(30, 30, 30)
            };

            rightPanel.Controls.Add(_selectedColorTextBox);
            rightPanel.Controls.Add(_selectedColorBox);
            rightPanel.Controls.Add(CreateLabel("Selected Color Values:"));

            rightPanel.Controls.Add(_btnResetView);
            rightPanel.Controls.Add(_btnRefresh);
            rightPanel.Controls.Add(_pointSizeTrackBar);
            rightPanel.Controls.Add(_pointSizeLabel);

            rightPanel.Controls.Add(_sampleCountComboBox);
            rightPanel.Controls.Add(CreateLabel("Max Sample Count:"));

            rightPanel.Controls.Add(_projectionComboBox);
            rightPanel.Controls.Add(CreateLabel("3D Projection:"));

            mainSplit.Panel2.Controls.Add(rightPanel);

            Controls.Add(mainSplit);
            Controls.Add(_statusLabel);
        }

        private void FillControls()
        {
            _projectionComboBox.Items.Add("RGB Cube");
            _projectionComboBox.Items.Add("HSV Cylinder");
            _projectionComboBox.Items.Add("LAB Space");
            _projectionComboBox.Items.Add("YCbCr Space");
            _projectionComboBox.Items.Add("YUV Space");
            _projectionComboBox.Items.Add("CMYK C-M-K Space");
            _projectionComboBox.SelectedIndex = 0;

            _sampleCountComboBox.Items.Add("5,000");
            _sampleCountComboBox.Items.Add("10,000");
            _sampleCountComboBox.Items.Add("25,000");
            _sampleCountComboBox.Items.Add("50,000");
            _sampleCountComboBox.Items.Add("100,000");
            _sampleCountComboBox.Items.Add("Full Image.!");
            _sampleCountComboBox.SelectedIndex = 2;
        }

        private void WireEvents()
        {
            _btnRefresh.Click += async delegate
            {
                await RefreshDistributionAsync();
            };

            _btnResetView.Click += delegate
            {
                _viewer.ResetView();
            };

            _pointSizeTrackBar.Scroll += delegate
            {
                _pointSizeLabel.Text =
                    "Point Size: " + _pointSizeTrackBar.Value;
            };

            _viewer.PointSelected += Viewer_PointSelected;
        }

        private async Task RefreshDistributionAsync()
        {
            if (!_workspace.HasImage)
            {
                SetStatus("No image loaded.");
                return;
            }

            Bitmap snapshot = null;

            try
            {
                SetBusy(true);
                SetStatus("Building 3D image color distribution...");

                ImageColorDistribution3DSettings settings = GetSettings();

                snapshot = new Bitmap(_workspace.WorkingImage);

                ImageColorDistribution3DResult result =
                    await Task.Run(() =>
                    {
                        return _distributionService.BuildDistribution(
                            snapshot,
                            settings);
                    });

                _viewer.SetDistribution(result, settings.PointSize);

                SetStatus(
                    "3D distribution built: " +
                    result.SampledPointCount.ToString("N0") +
                    " samples from " +
                    result.OriginalPixelCount.ToString("N0") +
                    " pixels in " +
                    result.ProcessingMilliseconds +
                    " ms.");
            }
            catch (Exception ex)
            {
                SetStatus("Failed to build 3D distribution.");

                MessageBox.Show(
                    "Failed to build 3D color distribution.\n\n" + ex.Message,
                    "3D Color Distribution Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                if (snapshot != null)
                    snapshot.Dispose();

                SetBusy(false);
            }
        }

        private ImageColorDistribution3DSettings GetSettings()
        {
            return new ImageColorDistribution3DSettings
            {
                ProjectionType = SelectedProjection,
                MaxSampleCount = SelectedMaxSampleCount,
                PointSize = _pointSizeTrackBar.Value
            };
        }

        private ImageColorProjection3DType SelectedProjection
        {
            get
            {
                switch (_projectionComboBox.SelectedIndex)
                {
                    case 0:
                        return ImageColorProjection3DType.RgbCube;

                    case 1:
                        return ImageColorProjection3DType.HsvCylinder;

                    case 2:
                        return ImageColorProjection3DType.LabSpace;

                    case 3:
                        return ImageColorProjection3DType.YCbCrSpace;

                    case 4:
                        return ImageColorProjection3DType.YuvSpace;

                    case 5:
                        return ImageColorProjection3DType.CmykCmkSpace;

                    default:
                        return ImageColorProjection3DType.RgbCube;
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

        private void Viewer_PointSelected(object sender, ImageColorPoint3D point)
        {
            _selectedColorBox.BackColor = point.DisplayColor;

            string allValues =
                point.CoordinateText +
                Environment.NewLine +
                Environment.NewLine +
                ColorValueConversions.BuildFullColorDescription(point.DisplayColor);
            _selectedColorTextBox.Text = allValues;


            SetStatus("Selected color synchronized across color spaces.");
        }

        private Label CreateLabel(string text)
        {
            return new Label
            {
                Text = text,
                Dock = DockStyle.Top,
                Height = 24,
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

        private Button CreateButton(string text)
        {
            return new Button
            {
                Text = text,
                Dock = DockStyle.Top,
                Height = 32,
                BackColor = Color.FromArgb(63, 63, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9)
            };
        }

        private void SetBusy(bool busy)
        {
            Cursor = busy ? Cursors.WaitCursor : Cursors.Default;

            _btnRefresh.Enabled = !busy;
            _btnResetView.Enabled = !busy;
            _projectionComboBox.Enabled = !busy;
            _sampleCountComboBox.Enabled = !busy;
            _pointSizeTrackBar.Enabled = !busy;
        }

        private void SetStatus(string message)
        {
            _statusLabel.Text = message;
        }
    }
}