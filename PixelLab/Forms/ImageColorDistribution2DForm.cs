using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using PixelLab.Controls;
using PixelLab.Models;
using PixelLab.Servicess;

namespace PixelLab.Forms
{
    public class ImageColorDistribution2DForm : Form
    {
        private readonly PixelLabWorkspace _workspace;
        private readonly ImageColorDistribution2DService _distributionService;

        private readonly ImageColorDistribution2DPanelControl _distributionPanel;
        private readonly Label _statusLabel;

        public ImageColorDistribution2DForm(PixelLabWorkspace workspace)
        {
            if (workspace == null)
                throw new ArgumentNullException(nameof(workspace));

            _workspace = workspace;
            _distributionService = new ImageColorDistribution2DService();

            Text = "2D Image Color Distribution";
            Width = 900;
            Height = 700;
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(750, 550);
            BackColor = Color.FromArgb(45, 45, 48);

            _distributionPanel = new ImageColorDistribution2DPanelControl
            {
                Dock = DockStyle.Fill
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

            Controls.Add(_distributionPanel);
            Controls.Add(_statusLabel);

            _distributionPanel.RefreshRequested += DistributionPanel_RefreshRequested;
            _distributionPanel.SetPanelEnabled(_workspace.HasImage);
        }

        private async void DistributionPanel_RefreshRequested(object sender, EventArgs e)
        {
            await RefreshDistributionAsync();
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
                SetStatus("Building 2D color distribution...");

                ImageColorDistribution2DSettings settings =
                    _distributionPanel.GetSettings();

                snapshot = new Bitmap(_workspace.WorkingImage);

                ImageColorDistribution2DResult result =
                    await Task.Run(() =>
                    {
                        return _distributionService.BuildDistribution(
                            snapshot,
                            settings);
                    });

                _distributionPanel.SetDistribution(result);

                SetStatus(
                    "Distribution built: " +
                    result.SampledPointCount.ToString("N0") +
                    " samples from " +
                    result.OriginalPixelCount.ToString("N0") +
                    " pixels in " +
                    result.ProcessingMilliseconds +
                    " ms.");
            }
            catch (Exception ex)
            {
                SetStatus("Failed to build distribution.");

                MessageBox.Show(
                    "Failed to build 2D color distribution.\n\n" + ex.Message,
                    "2D Color Distribution Error",
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

        private void SetBusy(bool isBusy)
        {
            Cursor = isBusy ? Cursors.WaitCursor : Cursors.Default;
            _distributionPanel.SetPanelEnabled(!isBusy && _workspace.HasImage);
        }

        private void SetStatus(string message)
        {
            _statusLabel.Text = message;
        }
    }
}