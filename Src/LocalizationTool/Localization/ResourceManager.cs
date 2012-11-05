using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using apcurium.Tools.Localization;
using apcurium.Tools.Localization.iOS;
using apcurium.Tools.Localization.Android;

namespace apcurium.Tools.Localization
{
    public class ResourceManager
    {
        private static readonly Dictionary<ResourceType, Type> _fileHandlers;

        static ResourceManager()
        {
            _fileHandlers = new Dictionary<ResourceType, Type>();
            _fileHandlers.Add(ResourceType.iOS, typeof(iOSResourceFileHandler));
            _fileHandlers.Add(ResourceType.Android, typeof(AndroidResourceFileHandler));
        }

        public ResourceManager()
        {
            Files = Settings.Default.LocalizedFiles.Select(r => 
                (ResourceFileHandlerBase) Activator.CreateInstance(_fileHandlers[r.Type], r.Path)).ToArray();
        }

        public ResourceFileHandlerBase[] Files { get; private set; }

    }
}
