using System;
using System.Drawing;
using System.Windows.Forms;
using PixelLab.Models;

namespace PixelLab.Forms
{
    public class BeforeAfterForm : Form
    {
        private readonly PixelLabWorkspace _workspace;

        private readonly PictureBox _beforePictureBox;
        private readonly PictureBox _afterPictureBox;
        private readonly ComboBox _afterSourceComboBox;
        private readonly Label _statusLabel;

        public BeforeAfterForm(PixelLabWorkspace workspace)
        {
            if (workspace == null)
                throw new ArgumentNullException(nameof(workspace));

            _workspace = workspace;

            Text = "Before / After Comparison";
            Width = 1100;
            Height = 700;
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(800, 500);
            BackColor = Color.FromArgb(45, 45, 48);

            _beforePictureBox = CreatePictureBox();
            _afterPictureBox = CreatePictureBox();

            _afterSourceComboBox = new ComboBox
            {
                Dock = DockStyle.Right,
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9)
            };

            _statusLabel = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 28,
                Text = "Before: Original Image | After: Working Image",
                ForeColor = Color.White,
                BackColor = Color.FromArgb(0, 122, 204),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0)
            };

            BuildLayout();
            FillControls();
            WireEvents();
            RefreshComparison();
        }

        private void BuildLayout()
        {
            Panel topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = Color.FromArgb(30, 30, 30),
                Padding = new Padding(8)
            };

            Label afterSourceLabel = new Label
            {
                Text = "After Source:",
                Dock = DockStyle.Right,
                Width = 90,
                ForeColor = Color.Gainsboro,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            topPanel.Controls.Add(_afterSourceComboBox);
            topPanel.Controls.Add(afterSourceLabel);

            SplitContainer split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = Width / 2,
                BackColor = Color.FromArgb(45, 45, 48)
            };

            Panel beforePanel = CreateImagePanel("Before - Original", _beforePictureBox);
            Panel afterPanel = CreateImagePanel("After", _afterPictureBox);

            split.Panel1.Controls.Add(beforePanel);
            split.Panel2.Controls.Add(afterPanel);

            Controls.Add(split);
            Controls.Add(topPanel);
            Controls.Add(_statusLabel);
        }

        private Panel CreateImagePanel(string title, PictureBox pictureBox)
        {
            Panel panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(25, 25, 25),
                Padding = new Padding(5)
            };

            Label titleLabel = new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                Height = 28,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            panel.Controls.Add(pictureBox);
            panel.Controls.Add(titleLabel);

            return panel;
        }

        private PictureBox CreatePictureBox()
        {
            return new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Black
            };
        }

        private void FillControls()
        {
            _afterSourceComboBox.Items.Add("Working Image");
            _afterSourceComboBox.Items.Add("Current Display Image");
            _afterSourceComboBox.SelectedIndex = 0;
        }

        private void WireEvents()
        {
            _afterSourceComboBox.SelectedIndexChanged += delegate
            {
                RefreshComparison();
            };
        }

        private void RefreshComparison()
        {
            if (!_workspace.HasImage)
                return;

            SetPictureBoxImage(_beforePictureBox, _workspace.OriginalImage);

            if (_afterSourceComboBox.SelectedIndex == 1)
            {
                SetPictureBoxImage(_afterPictureBox, _workspace.CurrentDisplayImage);
                _statusLabel.Text = "Before: Original Image | After: Current Display Image";
            }
            else
            {
                SetPictureBoxImage(_afterPictureBox, _workspace.WorkingImage);
                _statusLabel.Text = "Before: Original Image | After: Working Image";
            }
        }

        private void SetPictureBoxImage(PictureBox pictureBox, Image image)
        {
            if (pictureBox.Image != null)
            {
                Image old = pictureBox.Image;
                pictureBox.Image = null;
                old.Dispose();
            }

            if (image != null)
                pictureBox.Image = new Bitmap(image);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_beforePictureBox.Image != null)
            {
                Image old = _beforePictureBox.Image;
                _beforePictureBox.Image = null;
                old.Dispose();
            }

            if (_afterPictureBox.Image != null)
            {
                Image old = _afterPictureBox.Image;
                _afterPictureBox.Image = null;
                old.Dispose();
            }

            base.OnFormClosing(e);
        }
    }
}