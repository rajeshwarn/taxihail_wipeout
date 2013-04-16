using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;

namespace apcurium.MK.Booking.ConfigTool
{
    public class ConfigPList: Config
    {
        public ConfigPList(AppConfig parent)
            : base(parent)
        {
            KeySubPath = "";
        }

        public string Destination{ get; set; }

        public string Key{ get; set; }

        public string KeySubPath{ get; set; }

        public Action<XmlElement> SetterEle { get; set; }

        public override void Apply()
        {
            
            var destPath = Path.Combine(Parent.SrcDirectoryPath, PathConverter.Convert(Destination));

            var doc = new XmlDocument();

            doc.Load(destPath);

            var node = doc.SelectSingleNode("//key[. = '" + Key + "']");           

            SetterEle(node.NextSibling  as XmlElement);

            var w = new NullSubsetXmlTextWriter(destPath, Encoding.UTF8);
			w.Formatting = Formatting.Indented;
			doc.Save( w );
            w.Close();
            w = null;            
            
        }

        private class NullSubsetXmlTextWriter : XmlTextWriter
        {
            public NullSubsetXmlTextWriter(String inputFileName, Encoding encoding)
            : base(inputFileName, encoding)
            {
            }

            public override void WriteDocType(string name, string pubid, string sysid, string subset)
            {
                if (subset == String.Empty)
                {
                    subset = null;
                }
                base.WriteDocType(name, pubid, sysid, subset);
            }
        }

		public override string ToString ()
		{
			return string.Format ("[ConfigPList: Destination={0}, Key={1}, KeySubPath={2}, SetterEle={3}]", Destination, Key, KeySubPath, SetterEle);
		}
    }


}