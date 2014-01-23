using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;

namespace apcurium.MK.Booking.ConfigTool
{
    public class ConfigXML : Config
    {
		public ConfigXML(AppConfig parent)
            : base(parent)
        {
        }

        public string Destination{ get; set; }

		public Action<AppConfigFile, XmlAttribute> SetterAtt { get; set; }
		public Action<AppConfigFile, XmlElement> SetterEle { get; set; }

        public string NodeSelector { get; set; }
        public string Attribute { get; set; }

        public override void Apply()
        {
            
            var destPath = Path.Combine(Parent.SrcDirectoryPath, PathConverter.Convert( Destination));
            var doc = new XmlDocument();
            doc.Load(destPath);

            XmlNamespaceManager nsManager = new XmlNamespaceManager(doc.NameTable);
            nsManager.AddNamespace("a", "http://schemas.microsoft.com/developer/msbuild/2003");
            nsManager.AddNamespace("android", "http://schemas.android.com/apk/res/android");

            var node = doc.SelectSingleNode(NodeSelector, nsManager  ); 

            if (string.IsNullOrEmpty(Attribute))
            {
                SetterEle(Parent.Config, node as XmlElement);
            }
            else
            {
                XmlAttribute att = node.Attributes[Attribute];
				SetterAtt(Parent.Config, att);
            }

			using (var writer = new XmlTextWriter(destPath, new UTF8Encoding(false)))
			{
				writer.Formatting = Formatting.Indented;
				doc.Save(writer);
			}           
            
        }

		public override string ToString ()
		{
			return string.Format ("[ConfigXML: Destination={0}, SetterAtt={1}, SetterEle={2}, NodeSelector={3}, Attribute={4}]", Destination, SetterAtt, SetterEle, NodeSelector, Attribute);
		}
    }
}
