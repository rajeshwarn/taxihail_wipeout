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
       

        public AndroidResourceFileHandler(string filePath) : base(filePath)
        {
            var document = XElement.Load(filePath);
        

            foreach (var localization in document.Elements())
            {
                var key = localization.FirstAttribute.Value;

                TryAdd(key, localization.Value);
            }
        }

    }
}
