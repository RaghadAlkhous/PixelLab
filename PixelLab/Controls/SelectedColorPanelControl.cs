using System.Drawing;
using System.Windows.Forms;
using PixelLab.Models;

namespace PixelLab.Controls
{
    public class SelectedColorPanelControl : UserControl
    {
        private readonly Panel _colorPreviewPanel;
        private readonly TextBox _valuesTextBox;
        private readonly Label _sourceLabel;

        public SelectedColorPanelControl()
        {
            Dock = DockStyle.Top;
            Height = 260;
            BackColor = Color.FromArgb(30, 30, 30);

            Label title = new Label
            {
                Text = "Selected Color Synchronization",
                Dock = DockStyle.Top,
                Height = 30,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };

            _colorPreviewPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 45,
                BackColor = Color.Black,
                BorderStyle = BorderStyle.FixedSingle
            };

            _sourceLabel = new Label
            {
                Dock = DockStyle.Top,
                Height = 28,
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 8),
                TextAlign = ContentAlignment.MiddleLeft
            };

            _valuesTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                Font = new Font("Consolas", 9),
                BorderStyle = BorderStyle.FixedSingle
            };

            Controls.Add(_valuesTextBox);
            Controls.Add(_sourceLabel);
            Controls.Add(_colorPreviewPanel);
            Controls.Add(title);
        }

        public void SetSelectedColor(SelectedColorInfo info)
        {
            if (info == null)
            {
                Clear();
                return;
            }

            _colorPreviewPanel.BackColor = info.RgbColor;
            _sourceLabel.Text = "Source: " + info.SourceDescription;

            _valuesTextBox.Text =
                "RGB" + "\r\n" +
                "R = " + info.RgbColor.R +
                ", G = " + info.RgbColor.G +
                ", B = " + info.RgbColor.B +
                "\r\n\r\n" +

                "HSV" + "\r\n" +
                "H = " + info.H.ToString("0.0") + "°" +
                ", S = " + (info.S * 100.0).ToString("0.0") + "%" +
                ", V = " + (info.V * 100.0).ToString("0.0") + "%" +
                "\r\n\r\n" +

                "LAB" + "\r\n" +
                "L = " + info.L.ToString("0.0") +
                ", a = " + info.A.ToString("0.0") +
                ", b = " + info.LabB.ToString("0.0") +
                "\r\n\r\n" +

                "YCbCr" + "\r\n" +
                "Y = " + info.YCbCrY.ToString("0.0") +
                ", Cb = " + info.Cb.ToString("0.0") +
                ", Cr = " + info.Cr.ToString("0.0") +
                "\r\n\r\n" +

                "YUV" + "\r\n" +
                "Y = " + info.YuvY.ToString("0.0") +
                ", U = " + info.U.ToString("0.0") +
                ", V = " + info.YuvV.ToString("0.0") +
                "\r\n\r\n" +

                "CMYK Preview" + "\r\n" +
                "C = " + (info.C * 100.0).ToString("0.0") + "%" +
                ", M = " + (info.M * 100.0).ToString("0.0") + "%" +
                ", Y = " + (info.CmyY * 100.0).ToString("0.0") + "%" +
                ", K = " + (info.K * 100.0).ToString("0.0") + "%";
        }

        public void Clear()
        {
            _colorPreviewPanel.BackColor = Color.Black;
            _sourceLabel.Text = "";
            _valuesTextBox.Text = "";
        }
    }
}