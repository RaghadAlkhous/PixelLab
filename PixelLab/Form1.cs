using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using PixelLab.Servicess;
using PixelLab.Models;
using PixelLab.Forms;
using PixelLab.Utils;
using PixelLab.Services;

namespace PixelLab
{
    public partial class Form1 : Form
    {
        private string currentColorSpace = "RGB";

        private PixelLabWorkspace _workspace;
        private readonly ChannelProcessingService _channelProcessingService;

        public Form1()
        {
            _workspace = new PixelLabWorkspace();
            _channelProcessingService = new ChannelProcessingService();

            InitializeComponent();
            InitializeCustomSettings();
            UpdateCommandState();
        }

        private void InitializeCustomSettings()
        {
            pictureBoxMain.AllowDrop = true;
            pictureBoxMain.DragEnter += PictureBoxMain_DragEnter;
            pictureBoxMain.DragDrop += PictureBoxMain_DragDrop;
            cmbColorSpace.SelectedIndex = 0;
            lblStatus.Text = "Ready - Drag Image or Click Open";
        }

        #region تحميل وعرض الصور - الطلب الأول

        private void DisplayImage(Bitmap image)
        {
            if (image == null)
                return;
            if (pictureBoxMain.Image != null)
            {
                pictureBoxMain.Image.Dispose();
            }
            pictureBoxMain.Image = new Bitmap(image);
            pictureBoxMain.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxMain.Refresh();
            lblStatus.Text = $"Image Loaded: {image.Width}×{image.Height} pixels";
            UpdateCommandState();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                dlg.Title = "Select image to open it";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    LoadImage(dlg.FileName);
                }
            }
        }

        private void LoadImage(string filePath)
        {
            try
            {
                Bitmap image = new Bitmap(filePath);
                _workspace.LoadImage(image, filePath);
                DisplayImage(_workspace.CurrentDisplayImage);
                lblStatus.Text = $"Loaded: {Path.GetFileName(filePath)}";

                // تفعيل الأزرار والكومو بوكس في لوحة تعديل القناة بعد تحميل الصورة
                _channelPanel.SetPanelEnabled(true);
                _channelPanel.ResetSettings();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Loading Exception:\n{ex.Message}", "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PictureBoxMain_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length == 1)
                {
                    string ext = Path.GetExtension(files[0]).ToLower();
                    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" ||
                        ext == ".bmp" || ext == ".gif")
                    {
                        e.Effect = DragDropEffects.Copy;
                        return;
                    }
                }
            }
            e.Effect = DragDropEffects.None;
        }

        private void PictureBoxMain_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    LoadImage(files[0]);
                }
            }
        }

        #endregion

        #region تحويل أنظمة الألوان - الطلب الثاني

        private void cmbColorSpace_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_workspace.OriginalImage == null)
                return;
            currentColorSpace = cmbColorSpace.SelectedItem.ToString();
            try
            {
                switch (currentColorSpace)
                {
                    case "RGB":
                        _workspace.ReplaceWorkingImage(ColorConversionService.ConvertToRGB(_workspace.OriginalImage));
                        break;
                    case "CMY":
                        _workspace.ReplaceWorkingImage(ColorConversionService.ConvertToCMY(_workspace.OriginalImage));
                        break;
                    case "HSV":
                        _workspace.ReplaceWorkingImage(ColorConversionService.ConvertToHSV(_workspace.OriginalImage));
                        break;
                    case "YUV":
                        _workspace.ReplaceWorkingImage(ColorConversionService.ConvertToYUV(_workspace.OriginalImage));
                        break;
                    case "YCbCr":
                        _workspace.ReplaceWorkingImage(ColorConversionService.ConvertToYCbCr(_workspace.OriginalImage));
                        break;
                    case "LAB":
                        _workspace.ReplaceWorkingImage(ColorConversionService.ConvertToLAB(_workspace.OriginalImage));
                        break;
                }
                DisplayImage(_workspace.WorkingImage);
                lblStatus.Text = $"Converted to space: {currentColorSpace}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Conversion Error:\n{ex.Message}", "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region حفظ وإعادة تعيين وعرض المعلومات - الطلبات 9 و 10 و التعديل الجديد

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBoxMain.Image == null)
            {
                MessageBox.Show("No Image to save it..!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Filter = "JPEG Image|*.jpg|PNG Image|*.png|Bitmap Image|*.bmp";
                dlg.Title = "Save Image";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        pictureBoxMain.Image.Save(dlg.FileName);
                        lblStatus.Text = $"Saved: {Path.GetFileName(dlg.FileName)}";
                        MessageBox.Show("saved Successfully", "Sucess", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Saving Error:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_workspace.OriginalImage == null)
                return;

            _workspace.ReplaceWorkingImage(_workspace.OriginalImage);

            cmbColorSpace.SelectedIndex = 0;
            DisplayImage(_workspace.OriginalImage);
            _channelPanel.ResetSettings();

            lblStatus.Text = "Resetted";
        }

        private void imageInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!_workspace.HasImage)
            {
                MessageBox.Show("No image exists for displaying its info", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                string info = ImageInfoService.GetImageInfo(_workspace.WorkingImage, _workspace.CurrentFilePath);
                MessageBox.Show(info, "Iamge Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Displaying Error:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _workspace.Dispose();
            Application.Exit();
        }

        #endregion

        #region عرض مركبات كل نظام لوني والتحكم بها - الطلب الثالث

        private void ChannelPanel_SettingsChanged(object sender, EventArgs e)
        {
            ApplyChannelPreview();
        }

        private void ApplyChannelPreview()
        {
            if (!_workspace.HasImage)
                return;

            try
            {
                ChannelProcessingSettings settings = _channelPanel.GetSettings();

                using (ChannelProcessingResult result = _channelProcessingService.Process(_workspace.WorkingImage, settings))
                {
                    _workspace.ReplacePreviewImage(result.DisplayBitmap);

                    DisplayImage(_workspace.CurrentDisplayImage);

                    lblStatus.Text = result.Description + " completed in " + result.ProcessingMilliseconds + " ms.";
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Channel processing failed.";

                MessageBox.Show(
                    "Channel processing failed.\n\n" + ex.Message,
                    "Channel Processing Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void ChannelPanel_ClearPreviewRequested(object sender, EventArgs e)
        {
            if (!_workspace.HasImage)
                return;

            _workspace.ClearPreview();

            DisplayImage(_workspace.CurrentDisplayImage);

            lblStatus.Text = "Channel preview cleared.";
        }

        private void ChannelPanel_ApplyToWorkingRequested(object sender, EventArgs e)
        {
            if (!_workspace.HasImage)
                return;

            try
            {
                ChannelProcessingSettings settings = _channelPanel.GetSettings();

                /*
                 * عند التطبيق على WorkingImage نستخدم ReconstructedImage دائمًا.
                 * لأن تطبيق SingleChannel سيحوّل الصورة عمليًا إلى قناة واحدة رمادية.
                 */
                settings.ViewMode = ChannelViewMode.ReconstructedImage;

                using (ChannelProcessingResult result = _channelProcessingService.Process(_workspace.WorkingImage, settings))
                {
                    _workspace.ReplaceWorkingImage(result.DisplayBitmap);
                    _workspace.ClearPreview();

                    DisplayImage(_workspace.CurrentDisplayImage);

                    UpdateCommandState();

                    lblStatus.Text = "Channel changes applied to WorkingImage in " + result.ProcessingMilliseconds + " ms."; ;
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Failed to apply channel changes.";

                MessageBox.Show(
                    "Failed to apply channel changes.\n\n" + ex.Message,
                    "Apply Channel Changes Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        #endregion

        #region  تمثيل أنظمة الألوان  ضمن فضاءات ثنائية وثلاثية الأبعاد - الطلب الرابع والخامس

        private void Open2DColorSpaces_Click(object sender, EventArgs e)
        {
            if (!_workspace.HasImage)
            {
                lblStatus.Text = "Load an image first.";

                MessageBox.Show(
                    "Please load an image before opening the 2D color distribution window.",
                    "No Image",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            using (ImageColorDistribution2DForm form = new ImageColorDistribution2DForm(_workspace))
            {
                form.ShowDialog(this);
            }
        }

        private void Open3DColorSpaces_Click(object sender, EventArgs e)
        {
            if (!_workspace.HasImage)
            {
                lblStatus.Text = "Load an image first.";

                MessageBox.Show(
                    "Please load an image before opening the 3D color distribution window.",
                    "No Image",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            using (ImageColorDistribution3DForm form = new ImageColorDistribution3DForm(_workspace))
            {
                form.ShowDialog(this);
            }
        }
        #endregion

        #region الطلب السابع: انتقاء الألوان (Quantization)

        private void quantizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!_workspace.HasImage) { MessageBox.Show("Please Load Image First..!"); return; }

            int k = PromptForK();
            if (k < 2 || k > 256) return;

            Cursor = Cursors.WaitCursor;
            lblStatus.Text = $"Processing: {k} Color...";

            try
            {
                int stride;
                byte[] pixels = PixelProcessor.ReadPixels(_workspace.WorkingImage, out stride);
                QuantizationService.RunOptimizedKMeans(
                    pixels,
                    _workspace.WorkingImage.Width,
                    _workspace.WorkingImage.Height,
                    stride,
                    k,
                    out byte[] palette); 

                var lookup = QuantizationService.BuildLookup(palette);
                Bitmap preview = PixelProcessor.ApplyQuantizationWithLockBits(_workspace.WorkingImage, palette, lookup);

                _workspace.ReplacePreviewImage(preview);
                DisplayImage(_workspace.CurrentDisplayImage);

                lblStatus.Text = $"Processed: {k} color | Try again, cancel, or save the result.";
                ShowQuantizeButtons(k);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
                _workspace.ClearPreview();
                DisplayImage(_workspace.CurrentDisplayImage);
            }
            finally { Cursor = Cursors.Default; }
        }

        private int PromptForK()
        {
            using (var f = new Form())
            {
                f.Text = "Quantize Colors"; f.Size = new Size(300, 140);
                f.StartPosition = FormStartPosition.CenterParent;
                f.FormBorderStyle = FormBorderStyle.FixedDialog;
                f.MaximizeBox = false; f.MinimizeBox = false;

                var lbl = new Label { Text = "Colors Numbers (2-256):", Location = new Point(20, 20), AutoSize = true };
                var num = new NumericUpDown { Minimum = 2, Maximum = 256, Value = 8, Location = new Point(20, 45), Width = 100 };
                var ok = new Button { Text = "Ok", DialogResult = DialogResult.OK, Location = new Point(140, 40), Width = 80 };
                var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Location = new Point(140, 70), Width = 80 };

                f.Controls.AddRange(new Control[] { lbl, num, ok, cancel });
                f.AcceptButton = ok; f.CancelButton = cancel;

                return f.ShowDialog(this) == DialogResult.OK ? (int)num.Value : -1;
            }
        }

        private void ShowQuantizeButtons(int k)
        {
            CleanupQuantizeButtons();

            var apply = new Button { Name = "btnQApply", Text = " Apply", Dock = DockStyle.Top, Height = 30, BackColor = Color.DarkOliveGreen };
            apply.Click += (s, e) => {
                _workspace.ReplaceWorkingImage(_workspace.PreviewImage);
                _workspace.ClearPreview();
                DisplayImage(_workspace.CurrentDisplayImage);
                CleanupQuantizeButtons();
                lblStatus.Text = "Applied on \"WorkingImage\"";
            };

            var cancelBtn = new Button { Name = "btnQCancel", Text = "Cancel ", Dock = DockStyle.Top, Height = 30, BackColor = Color.Coral };
            cancelBtn.Click += (s, e) => {
                _workspace.ClearPreview();
                DisplayImage(_workspace.CurrentDisplayImage);
                CleanupQuantizeButtons();
                lblStatus.Text = "Canceled";
            };

            var retry = new Button { Name = "btnQRetry", Text = $"Try ({k})", Dock = DockStyle.Top, Height = 30, BackColor = Color.RoyalBlue };
            retry.Click += (s, e) => {
                _workspace.ClearPreview();
                DisplayImage(_workspace.CurrentDisplayImage);
                quantizeToolStripMenuItem_Click(null, EventArgs.Empty); 
            };

            panelControls.Controls.Add(apply);
            panelControls.Controls.Add(cancelBtn);
            panelControls.Controls.Add(retry);
            apply.BringToFront(); cancelBtn.BringToFront(); retry.BringToFront();
        }

        private void CleanupQuantizeButtons()
        {
            foreach (var name in new[] { "btnQApply", "btnQCancel", "btnQRetry" })
            {
                var c = panelControls.Controls.Find(name, false);
                if (c.Length > 0) { panelControls.Controls.Remove(c[0]); c[0].Dispose(); }
            }
        }

        #endregion

        #region إضافي: مقارنة قبل وبعد

        private void BtnBeforeAfter_Click(object sender, EventArgs e)
        {
            if (!_workspace.HasImage)
            {
                lblStatus.Text = "Load an image first.";

                MessageBox.Show(
                    "Please load an image before opening before/after comparison.",
                    "No Image",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            using (BeforeAfterForm form = new BeforeAfterForm(_workspace))
            {
                form.ShowDialog(this);
            }
        }

        #endregion

        private void UpdateCommandState()
        {
            bool hasImage = _workspace.HasImage;

            //_btnReset.Enabled = hasImage;
            //_btnSave.Enabled = hasImage;

            //_colorSpacePanel.SetPanelEnabled(hasImage);
            cmbColorSpace.Enabled = hasImage;
            saveToolStripMenuItem.Enabled = hasImage;
            imageInfoToolStripMenuItem.Enabled = hasImage;
            resetToolStripMenuItem.Enabled = hasImage;
            quantizeToolStripMenuItem.Enabled = hasImage;
            _channelPanel.SetPanelEnabled(hasImage);
            visualizing2DToolStripMenuItem.Enabled = hasImage;
            visualizing3DToolStripMenuItem.Enabled = hasImage;
            beforeAfterToolStripMenuItem.Enabled = hasImage;
        }
    }
}