using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace apcurium.Tools.Localization.Android
{
    //\mk-taxi\Src\Mobile\Android\Resources\Values\String.xml
    public class AndroidResourceFileHandler : ResourceFileHandlerBase
    {
        public AndroidResourceFileHandler(string filePath)
            : base(filePath)
        {
            var document = XElement.Load(filePath);

            foreach (var localization in document.Elements().Where(e => e.Name.ToString().Equals("string", StringComparison.OrdinalIgnoreCase)))
            {
                var key = localization.FirstAttribute.Value;

                TryAdd(key, localization.Value);
            }
        }

        protected override string GetFileText()
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<resources>\r\n");

            //<string name="ApplicationName">TaxiHail</string>
            foreach (var resource in this)
            {
                stringBuilder.AppendFormat("  <string name=\"{0}\">{1}</string>\r\n", resource.Key, resource.Value.Replace(">", "&gt;"));
            }

            stringBuilder.Append("</resources>");

            return stringBuilder.ToString();
        }
    }
}
