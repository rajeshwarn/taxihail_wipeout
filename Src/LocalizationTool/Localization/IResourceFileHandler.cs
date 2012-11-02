using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.Tools.Localization
{
    public interface IResourceFileHandler
    {
        void Load(string fileName);

        string Name { get; }
        
        string[] Keys { get; }

        string GetValue(string key);
        
        void SetValue(string key, string value);

        void Save();
    }
}
