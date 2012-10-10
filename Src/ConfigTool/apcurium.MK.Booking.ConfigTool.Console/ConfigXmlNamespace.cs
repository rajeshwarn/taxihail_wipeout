using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;
namespace apcurium.MK.Booking.ConfigTool
{
    public class ConfigXmlNamespace: Config
    {
        public ConfigXmlNamespace(AppConfig parent)
            : base(parent)
        {
        }

        public string Destination { get; set; }
        public string Namespace{ get; set; }
        public string Value { get; set; }

        public override void Apply()
        {
            
            var destPath = Path.Combine(Parent.SrcDirectoryPath, PathConverter.Convert( Destination));
            
            var file = File.ReadAllText( destPath );
            var f = Regex.Replace(file, Namespace + @"=""([^""]+)""", Namespace + @"=""http://schemas.android.com/apk/res/" + Value+ @""""); // .IsMatch(s, sPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase)


            using (var writer = new StreamWriter(destPath, false))
            {
                writer.Write(f);
                writer.Close();                
            }

        }

		public override string ToString ()
		{
			return string.Format ("[ConfigXmlNamespace: Destination={0}, Namespace={1}, Value={2}]", Destination, Namespace, Value);
		}
        
    }
}
 