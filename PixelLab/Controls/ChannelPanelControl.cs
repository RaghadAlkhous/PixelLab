using System;
using System.Drawing;
using System.Windows.Forms;
using PixelLab.Models;
using PixelLab.Servicess;

namespace PixelLab.Controls
{
    public class ChannelPanelControl : UserControl
    {
        private readonly ComboBox _colorSpaceComboBox;
        private readonly ComboBox _viewModeComboBox;
        private readonly ComboBox _selectedChannelComboBox;

        private readonly ChannelRow[] _rows;

        private readonly Button _btnClearPreview;
        private readonly Button _btnApplyToWorking;
        private readonly Button _btnResetSettings;

        private bool _suppressEvents;

        public event EventHandler SettingsChanged;
        public event EventHandler ClearPreviewRequested;
        public event EventHandler ApplyToWorkingRequested;

        public ChannelPanelControl()
        {
            Dock = DockStyle.Top;
            Height = 400;
            BackColor = Color.FromArgb(30, 30, 30);

            _rows = new ChannelRow[4];

            var title = new Label
            {
                Margin = new Padding(5, 5, 5, 5), // left, top, right, bottom,
                Text = "Channel Processing",
                Dock = DockStyle.Top,
                Height = 30,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };

            var colorSpaceLabel = CreateLabel("Channel Color Space:");
            _colorSpaceComboBox = CreateComboBox();

            _colorSpaceComboBox.Items.Add("RGB");
            _colorSpaceComboBox.Items.Add("Grayscale");
            _colorSpaceComboBox.Items.Add("HSV");
            _colorSpaceComboBox.Items.Add("LAB");
            _colorSpaceComboBox.Items.Add("YCbCr / YCrCb");
            _colorSpaceComboBox.Items.Add("YUV");
            _colorSpaceComboBox.Items.Add("CMYK");
            _colorSpaceComboBox.SelectedIndex = 0;

            var viewModeLabel = CreateLabel("View Mode:");
            _viewModeComboBox = CreateComboBox();

            _viewModeComboBox.Items.Add("Reconstructed Image");
            _viewModeComboBox.Items.Add("Single Channel");
            _viewModeComboBox.SelectedIndex = 0;

            var selectedChannelLabel = CreateLabel("Selected Channel:");
            _selectedChannelComboBox = CreateComboBox();

            var rowsPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 140,
                BackColor = Color.FromArgb(30, 30, 30)
            };
            var spacerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 10,
                BackColor = Color.FromArgb(30, 30, 30)
            };

            for (int i = 0; i < 4; i++)
            {
                _rows[i] = new ChannelRow(i);
                _rows[i].Panel.Dock = DockStyle.Top;
                _rows[i].Panel.Height = 36;

                _rows[i].Changed += Row_Changed;

                rowsPanel.Controls.Add(_rows[i].Panel);
                _rows[i].Panel.BringToFront();
            }
            rowsPanel.Margin = new Padding(0, 0, 0, 20);
            _btnResetSettings = CreateButton("Reset Channel Settings");
            _btnApplyToWorking = CreateButton("Apply to Working Image");
            _btnClearPreview = CreateButton("Clear Channel Preview");

            Controls.Add(_btnClearPreview);
            Controls.Add(_btnApplyToWorking);
            Controls.Add(_btnResetSettings);
            Controls.Add(spacerPanel);
            Controls.Add(rowsPanel);
            Controls.Add(_selectedChannelComboBox);
            Controls.Add(selectedChannelLabel);
            Controls.Add(_viewModeComboBox);
            Controls.Add(viewModeLabel);
            Controls.Add(_colorSpaceComboBox);
            Controls.Add(colorSpaceLabel);
            Controls.Add(title);

            _colorSpaceComboBox.SelectedIndexChanged += delegate
            {
                UpdateChannelLayout();
                RaiseSettingsChanged();
            };

            _viewModeComboBox.SelectedIndexChanged += delegate
            {
                UpdateSelectedChannelVisibility();
                RaiseSettingsChanged();
            };

            _selectedChannelComboBox.SelectedIndexChanged += delegate
            {
                RaiseSettingsChanged();
            };

            _btnResetSettings.Click += delegate
            {
                ResetSettings();
                RaiseSettingsChanged();
            };

            _btnClearPreview.Click += delegate
            {
                if (ClearPreviewRequested != null)
                    ClearPreviewRequested(this, EventArgs.Empty);
            };

            _btnApplyToWorking.Click += delegate
            {
                if (ApplyToWorkingRequested != null)
                    ApplyToWorkingRequested(this, EventArgs.Empty);
            };

            UpdateChannelLayout();
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

        private Button CreateButton(string text)
        {
            return new Button
            {
                Text = text,
                Dock = DockStyle.Top,
                Height = 25,
                BackColor = Color.FromArgb(63, 63, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9)
            };
        }

        private void Row_Changed(object sender, EventArgs e)
        {
            RaiseSettingsChanged();
        }

        private void RaiseSettingsChanged()
        {
            if (_suppressEvents)
                return;

            if (SettingsChanged != null)
                SettingsChanged(this, EventArgs.Empty);
        }

        private void UpdateChannelLayout()
        {
            _suppressEvents = true;

            try
            {
                ColorSpaceType colorSpace = SelectedColorSpace;

                ColorSpaceChannelInfo info = ColorSpaceChannelInfoProvider.GetInfo(colorSpace);

                _selectedChannelComboBox.Items.Clear();

                for (int i = 0; i < 4; i++)
                {
                    if (i < info.ChannelCount)
                    {
                        _rows[i].SetChannelName(info.ChannelNames[i]);
                        _rows[i].Panel.Visible = true;

                        _selectedChannelComboBox.Items.Add(
                            info.ChannelNames[i]);
                    }
                    else
                    {
                        _rows[i].Panel.Visible = false;
                    }

                    _rows[i].SetEnabledChecked(true);
                    _rows[i].SetOffset(0);
                }

                if (_selectedChannelComboBox.Items.Count > 0)
                    _selectedChannelComboBox.SelectedIndex = 0;

                UpdateSelectedChannelVisibility();
            }
            finally
            {
                _suppressEvents = false;
            }
        }

        private void UpdateSelectedChannelVisibility()
        {
            bool singleChannel =
                SelectedViewMode == ChannelViewMode.SingleChannel;

            _selectedChannelComboBox.Enabled = singleChannel;
        }

        public ChannelProcessingSettings GetSettings()
        {
            ChannelProcessingSettings settings = new ChannelProcessingSettings();

            settings.ColorSpace = SelectedColorSpace;
            settings.ViewMode = SelectedViewMode;
            settings.SelectedChannelIndex = (
                _selectedChannelComboBox.SelectedIndex < 0 ? 0 : _selectedChannelComboBox.SelectedIndex
            );

            for (int i = 0; i < 4; i++)
            {
                settings.ChannelEnabled[i] = _rows[i].IsChannelEnabled;
                settings.ChannelOffsets[i] = _rows[i].Offset;
            }

            return settings;
        }

        public ColorSpaceType SelectedColorSpace
        {
            get
            {
                switch (_colorSpaceComboBox.SelectedIndex)
                {
                    case 0:
                        return ColorSpaceType.RgbOriginal;

                    case 1:
                        return ColorSpaceType.Grayscale;

                    case 2:
                        return ColorSpaceType.Hsv;

                    case 3:
                        return ColorSpaceType.Lab;

                    case 4:
                        return ColorSpaceType.YCbCr;

                    case 5:
                        return ColorSpaceType.Yuv;

                    case 6:
                        return ColorSpaceType.CmykPreview;

                    default:
                        return ColorSpaceType.RgbOriginal;
                }
            }
        }

        public ChannelViewMode SelectedViewMode
        {
            get
            {
                if (_viewModeComboBox.SelectedIndex == 1)
                    return ChannelViewMode.SingleChannel;

                return ChannelViewMode.ReconstructedImage;
            }
        }

        public void ResetSettings()
        {
            _suppressEvents = true;

            try
            {
                _colorSpaceComboBox.SelectedIndex = 0;
                _viewModeComboBox.SelectedIndex = 0;

                for (int i = 0; i < 4; i++)
                {
                    _rows[i].SetEnabledChecked(true);
                    _rows[i].SetOffset(0);
                }

                UpdateChannelLayout();
            }
            finally
            {
                _suppressEvents = false;
            }
        }

        public void SetPanelEnabled(bool enabled)
        {
            _colorSpaceComboBox.Enabled = enabled;
            _viewModeComboBox.Enabled = enabled;
            _selectedChannelComboBox.Enabled = enabled && SelectedViewMode == ChannelViewMode.SingleChannel;

            for (int i = 0; i < 4; i++)
                _rows[i].SetRowEnabled(enabled);

            _btnClearPreview.Enabled = enabled;
            _btnApplyToWorking.Enabled = enabled;
            _btnResetSettings.Enabled = enabled;
        }

        private class ChannelRow
        {
            public Panel Panel { get; private set; }

            private readonly Label _nameLabel;
            private readonly CheckBox _enabledCheckBox;
            private readonly TrackBar _offsetTrackBar;
            private readonly Label _offsetValueLabel;

            public event EventHandler Changed;

            public ChannelRow(int index)
            {
                Panel = new Panel
                {
                    Height = 36,
                    BackColor = Color.FromArgb(30, 30, 30)
                };

                _nameLabel = new Label
                {
                    Dock = DockStyle.Left,
                    Width = 35,
                    ForeColor = Color.White,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold)
                };

                _enabledCheckBox = new CheckBox
                {
                    Dock = DockStyle.Left,
                    Width = 55,
                    Text = "On",
                    Checked = true,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 8)
                };

                _offsetValueLabel = new Label
                {
                    Dock = DockStyle.Right,
                    Width = 45,
                    Text = "0",
                    ForeColor = Color.White,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 8)
                };

                _offsetTrackBar = new TrackBar
                {
                    Dock = DockStyle.Fill,
                    Minimum = -100,
                    Maximum = 100,
                    Value = 0,
                    TickFrequency = 50,
                    SmallChange = 1,
                    LargeChange = 10
                };

                Panel.Controls.Add(_offsetTrackBar);
                Panel.Controls.Add(_offsetValueLabel);
                Panel.Controls.Add(_enabledCheckBox);
                Panel.Controls.Add(_nameLabel);

                _enabledCheckBox.CheckedChanged += delegate
                {
                    RaiseChanged();
                };

                _offsetTrackBar.Scroll += delegate
                {
                    _offsetValueLabel.Text =
                        _offsetTrackBar.Value.ToString();

                    RaiseChanged();
                };
            }

            public bool IsChannelEnabled
            {
                get { return _enabledCheckBox.Checked; }
            }

            public int Offset
            {
                get { return _offsetTrackBar.Value; }
            }

            public void SetChannelName(string name)
            {
                _nameLabel.Text = name;
            }

            public void SetEnabledChecked(bool isChecked)
            {
                _enabledCheckBox.Checked = isChecked;
            }

            public void SetOffset(int value)
            {
                if (value < _offsetTrackBar.Minimum)
                    value = _offsetTrackBar.Minimum;

                if (value > _offsetTrackBar.Maximum)
                    value = _offsetTrackBar.Maximum;

                _offsetTrackBar.Value = value;
                _offsetValueLabel.Text = value.ToString();
            }

            public void SetRowEnabled(bool enabled)
            {
                _enabledCheckBox.Enabled = enabled;
                _offsetTrackBar.Enabled = enabled;
            }

            private void RaiseChanged()
            {
                if (Changed != null)
                    Changed(this, EventArgs.Empty);
            }
        }
    }
}
