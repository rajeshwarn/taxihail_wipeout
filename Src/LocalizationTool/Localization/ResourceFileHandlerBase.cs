using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.Tools.Localization
{
    //Working with .resx Files Programmatically http://msdn.microsoft.com/en-us/library/gg418542.aspx
    public abstract class ResourceFileHandlerBase : Dictionary<string, string>
    {
        private readonly string _filePath;
        private readonly HashSet<string> _duplicateKeys;

        protected ResourceFileHandlerBase(string filePath)
        {
            _duplicateKeys = new HashSet<string>();
            _filePath = filePath;
        }

        public virtual string Name
        {
            get { return Path.GetFileName(_filePath); }
        }

        protected void TryAdd(string key, string value)
        {
            if (ContainsKey(key))
            {
                _duplicateKeys.Add(key);
            }
            else
            {
                Add(key, value);
            }
        }

        public ReadOnlyCollection<string> DuplicateKeys { get { return _duplicateKeys.ToList().AsReadOnly(); } }
    }
}
