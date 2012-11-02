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

        private static Dictionary<ResourceType, Type> _fileHandlers;
        static ResourceManager()
        {
            _fileHandlers = new Dictionary<ResourceType, Type>();
            _fileHandlers.Add( ResourceType.iOS, typeof( iOSResourceFileHandler));            
            _fileHandlers.Add(ResourceType.Android, typeof(AndroidResourceFileHandler));

        }
        
        public ResourceManager()
        {
            var files = new List<IResourceFileHandler>();
            foreach ( var r in Settings.Default.LocalizedFiles)
            {
                var h = (IResourceFileHandler)Activator.CreateInstance(_fileHandlers[r.Type]);
                h.Load( r.Path );
                files.Add(  h );
            }
            Files = files.ToArray();
        }

        public IResourceFileHandler[] Files { get; private set; }

    }
}
