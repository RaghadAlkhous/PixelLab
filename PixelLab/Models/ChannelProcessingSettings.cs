using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelLab.Models
{
    public class ChannelProcessingSettings
    {
        public ColorSpaceType ColorSpace { get; set; }
        public ChannelViewMode ViewMode { get; set; }
        public int SelectedChannelIndex { get; set; }

        public bool[] ChannelEnabled { get; set; }
        public int[] ChannelOffsets { get; set; }

        public ChannelProcessingSettings()
        {
            ColorSpace = ColorSpaceType.RgbOriginal;
            ViewMode = ChannelViewMode.ReconstructedImage;
            SelectedChannelIndex = 0;

            ChannelEnabled = new bool[] { true, true, true, true };
            ChannelOffsets = new int[] { 0, 0, 0, 0 };
        }

        public ChannelProcessingSettings Clone()
        {
            return new ChannelProcessingSettings
            {
                ColorSpace = ColorSpace,
                ViewMode = ViewMode,
                SelectedChannelIndex = SelectedChannelIndex,
                ChannelEnabled = (bool[])ChannelEnabled.Clone(),
                ChannelOffsets = (int[])ChannelOffsets.Clone()
            };
        }
    }
}
