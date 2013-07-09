using System;
using System.Collections.Generic;
using System.Drawing;
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
                basicSplashNinePatch.LeftRanges.Add(NinePatchRange.CreateFromEnd(1, 1d / 8d));

                basicSplashNinePatch.TopRanges.Add(NinePatchRange.CreateFromStart(0, 1d / 8d));
                basicSplashNinePatch.TopRanges.Add(NinePatchRange.CreateFromEnd(1, 1d / 8d));

                SaveDrawable(outputFolder,filename, "xhdpi", bitmap, basicSplashNinePatch, 1);
                SaveDrawable(outputFolder, filename, "hdpi", bitmap, basicSplashNinePatch, .75);
                SaveDrawable(outputFolder, filename, "mdpi", bitmap, basicSplashNinePatch, 5);
                SaveDrawable(outputFolder, filename, "ldpi", bitmap, basicSplashNinePatch, .375);

            }
        }


        private static void SaveDrawable(string folder, string filename, string suffix, Bitmap bitmap, NinePatch patch, double scale)
        {
            var newFilename = BuildFilename(folder, suffix, filename);

            Directory.CreateDirectory(Path.GetDirectoryName(newFilename));

            using (var newImage = Resizer.ResizeBitmap(bitmap, scale))
            {
                patch.DrawOn(newImage);
                newImage.Save(newFilename);
            }
        }

        private static string BuildFilename(string folder, string folderSuffix, string filename)
        {
            return folder +
                   "\\drawable-" + folderSuffix + "\\" +
                   Path.GetFileNameWithoutExtension(filename) + ".9" +
                   Path.GetExtension(filename);
        }
    }
}
