using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using PixelLab.Servicess;
using PixelLab.Models;
using PixelLab.Forms;

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

        }

        private void InitializeCustomSettings()
        {
            pictureBoxMain.AllowDrop = true;
            pictureBoxMain.DragEnter += PictureBoxMain_DragEnter;
            pictureBoxMain.DragDrop += PictureBoxMain_DragDrop;
            cmbColorSpace.SelectedIndex = 0;
            lblStatus.Text = "جاهز - اسحب صورة أو اضغط فتح";
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
            lblStatus.Text = $"تم تحميل الصورة: {image.Width}×{image.Height} بكسل";
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = "ملفات الصور|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                dlg.Title = "اختر صورة لفتحها";
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
                lblStatus.Text = $"تم تحميل: {Path.GetFileName(filePath)}";

                // تفعيل الأزرار والكومو بوكس في لوحة تعديل القناة بعد تحميل الصورة
                _channelPanel.SetPanelEnabled(true);
                _channelPanel.ResetSettings();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل الصورة:\n{ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                lblStatus.Text = $"تم التحويل إلى نظام: {currentColorSpace}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في التحويل:\n{ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region حفظ وإعادة تعيين وعرض المعلومات - الطلبات 9 و 10 و التعديل الجديد

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBoxMain.Image == null)
            {
                MessageBox.Show("لا توجد صورة لحفظها", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Filter = "JPEG Image|*.jpg|PNG Image|*.png|Bitmap Image|*.bmp";
                dlg.Title = "حفظ الصورة";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        pictureBoxMain.Image.Save(dlg.FileName);
                        lblStatus.Text = $"تم الحفظ: {Path.GetFileName(dlg.FileName)}";
                        MessageBox.Show("تم حفظ الصورة بنجاح", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"خطأ في الحفظ:\n{ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            lblStatus.Text = "تم إعادة التعيين";
        }

        private void imageInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_workspace.HasImage)
            {
                MessageBox.Show("لا توجد صورة محددة لعرض معلوماتها", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                string info = ImageInfoService.GetImageInfo(_workspace.WorkingImage, _workspace.CurrentFilePath);
                MessageBox.Show(info, "معلومات الصورة", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في عرض المعلومات:\n{ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            if (_workspace.HasImage)
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

        private void UpdateCommandState()
        {
            bool hasImage = _workspace.HasImage;

            //_btnReset.Enabled = hasImage;
            //_btnSave.Enabled = hasImage;

            //_colorSpacePanel.SetPanelEnabled(hasImage);
            _channelPanel.SetPanelEnabled(hasImage);
            visualizing2DToolStripMenuItem.Enabled = hasImage;
            visualizing3DToolStripMenuItem.Enabled = hasImage;
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
    }
}