﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ninePatchMaker
{
    public class NinePatchMaker
    {
        private NinePatchMaker()
        {}

        public static void Generate(string filePathToPatch, string outputFolder, string filename)
        {
            using (var bitmap = new Bitmap(filePathToPatch))
            {
                var basicSplashNinePatch = new NinePatch();

                basicSplashNinePatch.LeftRanges.Add(NinePatchRange.CreateFromStart(0, 1d / 8d));
                basicSplashNinePatch.LeftRanges.Add(NinePatchRange.CreateFromEnd(1, 1d / 26d));

                basicSplashNinePatch.TopRanges.Add(NinePatchRange.CreateFromStart(0, 1d / 8d));
                basicSplashNinePatch.TopRanges.Add(NinePatchRange.CreateFromEnd(1, 1d / 8d));

                basicSplashNinePatch.RightRanges.Add(NinePatchRange.CreateFromStart(1d / 8d, 6.7d / 8d));
                basicSplashNinePatch.BottomRanges.Add(NinePatchRange.CreateFromStart(1d / 8d, 6d / 8d));

				SaveDrawable(outputFolder, filename, "xxhdpi", bitmap, basicSplashNinePatch, 1.5);
				SaveDrawable(outputFolder, filename, "xhdpi", bitmap, basicSplashNinePatch, 1);
                SaveDrawable(outputFolder, filename, "hdpi", bitmap, basicSplashNinePatch, .75);
				SaveDrawable(outputFolder, filename, "mdpi", bitmap, basicSplashNinePatch, .5);
                SaveDrawable(outputFolder, filename, "ldpi", bitmap, basicSplashNinePatch, .375);
            }
        }

        private static void TagImage(Image bitmap, string text)
        {
            var rectf = new Rectangle((bitmap.Width / 2) - 90, (bitmap.Height / 2) - 50, 120, 30);

            var g = Graphics.FromImage(bitmap);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.FillRectangle(Brushes.White,rectf);

			g.DrawString(text, new Font("Comic Sans MS", 12), Brushes.Black, rectf);

            g.Flush();
        }

        private static void SaveDrawable(string folder, string filename, string suffix, Bitmap bitmap, NinePatch patch, double scale)
        {
            var newFilename = BuildFilename(folder, suffix, filename);
#if DEBUG
			//Todo Fonts dont work on Mac
            //TagImage(bitmap, suffix);
#endif
            Directory.CreateDirectory(Path.GetDirectoryName(newFilename));

            using (var newImage = Resizer.ResizeBitmap(bitmap, scale))
            {
                patch.DrawOn(newImage);
                newImage.Save(newFilename);
            }
        }

        private static string BuildFilename(string folder, string folderSuffix, string filename)
        {
			folder = Path.Combine(folder,"drawable-" + folderSuffix);
			var file = Path.Combine(folder,Path.GetFileNameWithoutExtension(filename) + ".9" +
			                        Path.GetExtension(filename));

            return file;
        }
    }
}
