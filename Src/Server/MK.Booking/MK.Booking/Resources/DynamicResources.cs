using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Resources
{
    public class DynamicResources: System.Dynamic.DynamicObject
    {
        readonly ResourceManager _resources;
        bool? _missingResourceFile;

        public DynamicResources(string applicationKey)
        {
            _resources = new ResourceManager("apcurium.MK.Booking.Resources." + applicationKey, GetType().Assembly);
        }

        public bool MissingResourceFile
        {
            get
            {
                if (!_missingResourceFile.HasValue)
                {
                    try
                    {
                        _resources.GetString("test");
                        _missingResourceFile = false;
                    }
                    catch (System.Resources.MissingManifestResourceException)
                    {
                        // It's all right, we'll use the Global resource file instead
                        _missingResourceFile = true;
                    }
                }
                return _missingResourceFile.Value;
            }
        }

        public string GetString(string key)
        {
            if (MissingResourceFile)
            {
                return Global.ResourceManager.GetString(key);
            }
            else
            {
                return _resources.GetString(key)
                    ?? Global.ResourceManager.GetString(key);
            }
        }

        public override bool TryGetMember(System.Dynamic.GetMemberBinder binder, out object result)
        {
            result = GetString(binder.Name);
            return true;
        } 


    }
}
