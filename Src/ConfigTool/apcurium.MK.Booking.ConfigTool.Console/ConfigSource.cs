using System;
using System.IO;

namespace apcurium.MK.Booking.ConfigTool
{
	public class ConfigSource: Config
	{
		public string Source { get; set; }
		public string ToReplace { get; set; }
		public string ReplaceWith { get; set; }

		public ConfigSource(AppConfig parent)
			: base(parent)
		{
		}

		public override void Apply()
		{
			var sourcePath = Path.Combine(Parent.SrcDirectoryPath, PathConverter.Convert( Source ) );
			var fileContents = File.ReadAllText(sourcePath);

			fileContents = fileContents.Replace(ToReplace, ReplaceWith); 

			File.WriteAllText(sourcePath, fileContents);
		}

		public override string ToString ()
		{
			return string.Format ("[ConfigSource: Source={0}, ToReplace={1}, ReplaceWith={2}]", Source, ToReplace, ReplaceWith);
		}

	}
}

