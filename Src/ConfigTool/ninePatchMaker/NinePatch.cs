using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ninePatchMaker
{
    public class NinePatch
    {
        public NinePatch()
        {
            TopRanges = new List<NinePatchRange>();
            LeftRanges = new List<NinePatchRange>();
            RightRanges = new List<NinePatchRange>();
            BottomRanges = new List<NinePatchRange>();
        }

        public List<NinePatchRange> TopRanges { get; set; }
        public List<NinePatchRange> LeftRanges { get; set; }


        public List<NinePatchRange> BottomRanges { get; set; }
        public List<NinePatchRange> RightRanges { get; set; }


        public void DrawOn(Bitmap bitmap)
        {
            DrawTop(bitmap);
            DrawLeft(bitmap);
            DrawRight(bitmap);
            DrawBottom(bitmap);

            //clear 4 corners
            bitmap.SetPixel(0, 0, Color.Transparent);
            bitmap.SetPixel(0, bitmap.Height-1, Color.Transparent);
            bitmap.SetPixel(bitmap.Width-1, 0, Color.Transparent);
            bitmap.SetPixel(bitmap.Width-1, bitmap.Height-1, Color.Transparent);
        }

        private void DrawBottom(Bitmap bitmap)
        {
            var y = bitmap.Height - 1;

            for (var i = 0; i < bitmap.Width; i++)
            {
                var x = i;
                var color = Color.Transparent;

                if (BottomRanges.Any(range => range.Contains(i, bitmap.Width)))
                {
                    color = Color.Black;
                }

                bitmap.SetPixel(x, y, color);
            }
        }

        private void DrawRight(Bitmap bitmap)
        {
            var x = bitmap.Width - 1;

            for (var i = 0; i < bitmap.Height; i++)
            {
                var y = i;
                var color = Color.Transparent;

                if (RightRanges.Any(range => range.Contains(i, bitmap.Height)))
                {
                    color = Color.Black;
                }


                bitmap.SetPixel(x, y, color);
            }
        }


        private void DrawLeft(Bitmap bitmap)
        {
            const int x = 0;
            for (var i = 0; i < bitmap.Height; i++)
            {
                var y = i;
                var color = Color.Transparent;

                if (LeftRanges.Any(range => range.Contains(i, bitmap.Height)))
                {
                    color = Color.Black;
                }

                bitmap.SetPixel(x, y, color);
            }
        }

        private void DrawTop(Bitmap bitmap)
        {
            const int y = 0;
            for (var i = 0; i < bitmap.Width; i++)
            {
                var x = i;
                var color = Color.Transparent;

                if (TopRanges.Any(range => range.Contains(i, bitmap.Width)))
                {
                    color = Color.Black;
                }

                bitmap.SetPixel(x, y, color);
            }
        }
    }
}
