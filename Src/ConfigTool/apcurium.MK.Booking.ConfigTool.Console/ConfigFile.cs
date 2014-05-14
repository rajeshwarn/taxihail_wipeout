using System.IO;

namespace apcurium.MK.Booking.ConfigTool
{
    public class ConfigFile : Config
    {
        public string Source { get; set; }
        public string Destination{ get; set; }

        public ConfigFile(AppConfig parent)
            : base(parent)
        {
        }

        public override void Apply()
        {
            var destPath = Path.Combine(Parent.SrcDirectoryPath, PathConverter.Convert(  Destination ) );

            //Todo : Here _filePathToPatch should be the equal to temp folder containing all the assets
            var sourcePath = Path.Combine(Parent.ConfigDirectoryPath, PathConverter.Convert(  Source ) );
            if (!File.Exists(sourcePath))
            {
                sourcePath = Path.Combine(Parent.CommonDirectoryPath, PathConverter.Convert(Source));
            }

			if (!File.Exists (sourcePath))
			{
				// source file not found, abort!
				return;
			}

            File.Copy(sourcePath, destPath, true);
        }

		public override string ToString ()
		{
			return string.Format ("[ConfigFile: Source={0}, Destination={1}]", Source, Destination);
		}
        
    }

}
