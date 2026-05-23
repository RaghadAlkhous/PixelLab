using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelLab.Models
{
    public class ColorSpaceChannelInfo
    {
        public ColorSpaceType ColorSpace { get; private set; }
        public string DisplayName { get; private set; }
        public string[] ChannelNames { get; private set; }

        public int ChannelCount
        {
            get { return ChannelNames.Length; }
        }

        public ColorSpaceChannelInfo(
            ColorSpaceType colorSpace,
            string displayName,
            string[] channelNames)
        {
            ColorSpace = colorSpace;
            DisplayName = displayName;
            ChannelNames = channelNames;
        }
    }
}
