using System;
using System.Drawing;

namespace PixelLab.Models
{
    public sealed class PixelLabWorkspace : IDisposable
    {
        public Bitmap OriginalImage { get; private set; }
        public Bitmap WorkingImage { get; private set; }
        public Bitmap PreviewImage { get; private set; }

        public string CurrentFilePath { get; private set; }

        public bool HasImage
        {
            get { return OriginalImage != null && WorkingImage != null; }
        }

        public bool HasPreview
        {
            get { return PreviewImage != null; }
        }

        public Bitmap CurrentDisplayImage
        {
            get
            {
                if (PreviewImage != null)
                    return PreviewImage;

                return WorkingImage;
            }
        }

        public void LoadImage(Bitmap image, string filePath)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            Clear();

            OriginalImage = new Bitmap(image);
            WorkingImage = new Bitmap(image);
            PreviewImage = null;
            CurrentFilePath = filePath;
        }

        public void ReplaceWorkingImage(Bitmap newImage)
        {
            if (newImage == null)
                throw new ArgumentNullException(nameof(newImage));

            if (WorkingImage != null)
                WorkingImage.Dispose();

            WorkingImage = new Bitmap(newImage);

            ClearPreview();
        }

        public void ReplacePreviewImage(Bitmap previewImage)
        {
            if (previewImage == null)
                throw new ArgumentNullException(nameof(previewImage));

            ClearPreview();

            this.PreviewImage = new Bitmap(previewImage);
        }

        public void ClearPreview()
        {
            if (PreviewImage != null)
            {
                PreviewImage.Dispose();
                PreviewImage = null;
            }
        }

        public void ResetToOriginal()
        {
            if (OriginalImage == null)
                return;

            if (WorkingImage != null)
                WorkingImage.Dispose();

            WorkingImage = new Bitmap(OriginalImage);

            ClearPreview();
        }

        public void Clear()
        {
            if (OriginalImage != null)
            {
                OriginalImage.Dispose();
                OriginalImage = null;
            }

            if (WorkingImage != null)
            {
                WorkingImage.Dispose();
                WorkingImage = null;
            }

            if (PreviewImage != null)
            {
                PreviewImage.Dispose();
                PreviewImage = null;
            }

            CurrentFilePath = null;
        }

        public void Dispose()
        {
            Clear();
        }
    }
}