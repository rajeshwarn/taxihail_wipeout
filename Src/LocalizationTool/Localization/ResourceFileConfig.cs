using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.Tools.Localization
{
  

    [Serializable]    
    public class ResourceFileConfig
    {
        public ResourceFileConfig()
        {

        }

        public string Path { get; set; }

        public ResourceType Type { get; set; }

        //public ResourceType ResourceType
        //{
        //    get { return (ResourceType) Enum.Parse(typeof (ResourceType), Type, true); }
        //}


    }
}
