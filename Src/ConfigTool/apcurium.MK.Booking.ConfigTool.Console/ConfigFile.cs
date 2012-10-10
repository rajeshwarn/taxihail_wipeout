using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            var sourcePath = Path.Combine(Parent.ConfigDirectoryPath, PathConverter.Convert(  Source ) );
            File.Copy(sourcePath, destPath, true);
        }

		public override string ToString ()
		{
			return string.Format ("[ConfigFile: Source={0}, Destination={1}]", Source, Destination);
		}
        
    }

}
