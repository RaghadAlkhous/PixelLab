using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace PixelLab.Models
{
    public sealed class ChannelProcessingResult : IDisposable
    {
        public Bitmap DisplayBitmap { get; private set; }
        public string Description { get; private set; }
        public long ProcessingMilliseconds { get; private set; }

        public ChannelProcessingResult(
            Bitmap displayBitmap,
            string description,
            long processingMilliseconds)
        {
            if (displayBitmap == null)
                throw new ArgumentNullException(nameof(displayBitmap));

            DisplayBitmap = displayBitmap;
            Description = description;
            ProcessingMilliseconds = processingMilliseconds;
        }

        public void Dispose()
        {
            if (DisplayBitmap != null)
            {
                DisplayBitmap.Dispose();
                DisplayBitmap = null;
            }
        }
    }
}
