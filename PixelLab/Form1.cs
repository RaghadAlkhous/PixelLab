using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using PixelLab.Servicess;
using PixelLab.Utils;

namespace PixelLab
{
    public partial class Form1 : Form
    {
        private Bitmap originalImage = null;
        private Bitmap currentImage = null;
        private string currentImagePath = string.Empty;
        private string currentColorSpace = "RGB";

        public Form1()
        {
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
                if (originalImage != null)
                {
                    originalImage.Dispose();
                }
                if (currentImage != null)
                {
                    currentImage.Dispose();
                }
                originalImage = new Bitmap(filePath);
                currentImage = new Bitmap(originalImage);
                currentImagePath = filePath;
                DisplayImage(currentImage);
                lblStatus.Text = $"تم تحميل: {Path.GetFileName(filePath)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل الصورة:\n{ex.Message}",
                    "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            if (originalImage == null)
                return;
            currentColorSpace = cmbColorSpace.SelectedItem.ToString();
            try
            {
                switch (currentColorSpace)
                {
                    case "RGB":
                        currentImage = ColorConversionService.ConvertToRGB(originalImage);
                        break;
                    case "CMY":
                        currentImage = ColorConversionService.ConvertToCMY(originalImage);
                        break;
                    case "HSV":
                        currentImage = ColorConversionService.ConvertToHSV(originalImage);
                        break;
                    case "YUV":
                        currentImage = ColorConversionService.ConvertToYUV(originalImage);
                        break;
                    case "YCbCr":
                        currentImage = ColorConversionService.ConvertToYCbCr(originalImage);
                        break;
                    case "LAB":
                        currentImage = ColorConversionService.ConvertToLAB(originalImage);
                        break;
                }
                DisplayImage(currentImage);
                lblStatus.Text = $"تم التحويل إلى نظام: {currentColorSpace}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في التحويل:\n{ex.Message}",
                    "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region حفظ وإعادة تعيين وعرض المعلومات - الطلبات 9 و 10 و التعديل الجديد

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBoxMain.Image == null)
            {
                MessageBox.Show("لا توجد صورة لحفظها", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                        MessageBox.Show("تم حفظ الصورة بنجاح", "نجاح",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"خطأ في الحفظ:\n{ex.Message}",
                            "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (originalImage == null)
                return;
            currentImage = new Bitmap(originalImage);
            cmbColorSpace.SelectedIndex = 0;
            DisplayImage(currentImage);
            lblStatus.Text = "تم إعادة التعيين";
        }

        private void imageInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentImage == null || string.IsNullOrEmpty(currentImagePath))
            {
                MessageBox.Show("لا توجد صورة محددة لعرض معلوماتها", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                string info = ImageInfoService.GetImageInfo(currentImage, currentImagePath);
                MessageBox.Show(info, "معلومات الصورة",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في عرض المعلومات:\n{ex.Message}",
                    "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (originalImage != null)
            {
                originalImage.Dispose();
            }
            if (currentImage != null)
            {
                currentImage.Dispose();
            }
            Application.Exit();
        }

        #endregion

        private void Form1_Load(object sender, EventArgs e)
        {
        }

    }
}