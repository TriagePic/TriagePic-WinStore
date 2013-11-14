using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialEbola.Lib.PopupHelpers
{
    [Flags]
    public enum PopupAnimation
    {
        ControlFlip = 0x1,
				ControlFlyoutRight = 0x2,

        OverlayFade = 0x1 << 16,
    }

    internal static partial class EnumExtensions
    {
        public static bool IsFlagOn(this PopupAnimation value, PopupAnimation flag)
        {
            return (value & flag) == flag;
        }
    }
}
