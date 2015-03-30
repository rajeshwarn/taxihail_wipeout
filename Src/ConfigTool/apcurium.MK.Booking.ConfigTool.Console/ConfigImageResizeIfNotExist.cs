using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;

namespace apcurium.MK.Booking.ConfigTool
{
    public class ConfigImageResizeIfNotExist : Config
    {
        public string Source { get; set; }
        public string Destination { get; set; }
        public string DefaultSource { get; set; }
        public SizeF OutputSize { get; set; }

        public ConfigImageResizeIfNotExist(AppConfig parent)
            : base(parent)
        {
        }

        public override void Apply()
        {
            var destPath = Path.Combine(Parent.SrcDirectoryPath, PathConverter.Convert(Destination));

            var sourcePath = Path.Combine(Parent.ConfigDirectoryPath, PathConverter.Convert(Source));
            if (!string.IsNullOrWhiteSpace(Parent.CommonDirectoryPath) && !File.Exists(sourcePath))
            {
                sourcePath = Path.Combine(Parent.CommonDirectoryPath, PathConverter.Convert(Source));
            }

            if (!File.Exists (sourcePath))
            {
                // original file not found, resizing default image
                var defaultSourcePath = Path.Combine(Parent.ConfigDirectoryPath, PathConverter.Convert(DefaultSource));
                var newImage = new Bitmap((int)OutputSize.Width, (int)OutputSize.Height, PixelFormat.Format24bppRgb);
                var defaultImage = Bitmap.FromFile(defaultSourcePath);

                // Draws the image in the specified size with quality mode set to HighQuality
                using (Graphics graphics = Graphics.FromImage(newImage))
                {
                    graphics.CompositingQuality = CompositingQuality.HighQuality;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.DrawImage(defaultImage, 0, 0, OutputSize.Width, OutputSize.Height);
                }

                newImage.Save(destPath, ImageFormat.Png);
            }
            else
            {
                File.Copy(sourcePath, destPath, true);
            }
        }

        public override string ToString ()
        {
            return string.Format ("[ConfigImageResizeIfNotExist: Source={0}, Destination={1}]", Source, Destination);
        }
    }
}
