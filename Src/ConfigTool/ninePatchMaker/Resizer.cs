using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ninePatchMaker
{
    class Resizer
    {
        public static Bitmap ResizeBitmap(string filename, Size desiredSize)
        {
            using (var bitmap = new Bitmap(filename))
            {
                return ResizeBitmap(bitmap, desiredSize);
            }
        }

        public static Bitmap ResizeBitmap(string filename, double percent)
        {
            using (var bitmap = new Bitmap(filename))
            {
                var desiredSize = new Size((int) (bitmap.Width*percent), (int) (bitmap.Height*percent));

                return ResizeBitmap(bitmap,desiredSize);
            }
        }

        public static Bitmap ResizeBitmap(Bitmap bitmap, Size desiredSize)
        {
            var b = new Bitmap(desiredSize.Width, desiredSize.Height);
            using (var g = Graphics.FromImage(b))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                g.DrawImage(bitmap, 0, 0, desiredSize.Width, desiredSize.Height);
            }
            return b;
        }

        public static Bitmap ResizeBitmap(Bitmap bitmap, double percent)
        {
            var desiredSize = new Size((int)(bitmap.Width * percent), (int)(bitmap.Height * percent));

            return ResizeBitmap(bitmap, desiredSize);
        }
    }
}
