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

        public Action<AppInfo, XmlAttribute> SetterAtt { get; set; }
        public Action<AppInfo, XmlElement> SetterEle { get; set; }

        public string NodeSelector { get; set; }
        public string Attribute { get; set; }

        public override void Apply()
        {
            
            var destPath = Path.Combine(Parent.SrcDirectoryPath, PathConverter.Convert( Destination));
            var doc = new XmlDocument();
            doc.Load(destPath);

            XmlNamespaceManager nsManager = new XmlNamespaceManager(doc.NameTable);
            nsManager.AddNamespace(
                         "a",
                         "http://schemas.microsoft.com/developer/msbuild/2003"
                          );

            var node = doc.SelectSingleNode(NodeSelector, nsManager  ); 

            if (string.IsNullOrEmpty(Attribute))
            {
                SetterEle(Parent.App, node as XmlElement);
            }
            else
            {
                XmlAttribute att = node.Attributes[Attribute];
                SetterAtt(Parent.App, att);
            }
            doc.Save(destPath);
            
            
        }
    }
}
