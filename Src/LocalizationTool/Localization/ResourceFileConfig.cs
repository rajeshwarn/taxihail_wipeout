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
        public string Path { get; set; }
        public ResourceType Type { get; set; }
    }
}
