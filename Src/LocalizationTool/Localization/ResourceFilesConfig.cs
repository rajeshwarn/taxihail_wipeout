using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace apcurium.Tools.Localization
{
    [Serializable]
    public class ResourceFilesConfig : List<ResourceFileConfig>,  IList
    {
        public ResourceFilesConfig ()
        {
            
        }

        
     
    }
}
