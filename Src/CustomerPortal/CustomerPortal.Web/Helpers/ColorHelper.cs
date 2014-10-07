using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;

namespace CustomerPortal.Web.Helpers
{
    public static class ColorHelper
    {
        public static string HexFromColor(System.Drawing.Color color)
        {
            return ColorTranslator.FromHtml(String.Format("#{0:X2}{1:X2}{2:X2}", color.R, color.G, color.B)).Name.Remove(0, 2);
        }

        public static System.Drawing.Color ColorFromHex(string myColor)
        {

            return System.Drawing.Color.FromArgb(
                        Convert.ToByte(myColor.Substring(1, 2), 16),
                        Convert.ToByte(myColor.Substring(3, 2), 16),
                        Convert.ToByte(myColor.Substring(5, 2), 16),
                        Convert.ToByte(myColor.Substring(7, 2), 16)
                    );
        }

    }
}