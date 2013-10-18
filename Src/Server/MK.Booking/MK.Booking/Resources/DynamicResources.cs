using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Resources
{
    public class DynamicResources: System.Dynamic.DynamicObject
    {
        readonly ResourceManager _resources;

        public DynamicResources(string applicationKey)
        {            
            var names = this.GetType().Assembly.GetManifestResourceNames();
            var resourceName = "apcurium.MK.Booking.Resources." + applicationKey + ".resources";
            
            MissingResourceFile = names.None(n => n.ToLower() == resourceName.ToLower()); // resource files doesn't exist for all the customers
            
            _resources = new ResourceManager("apcurium.MK.Booking.Resources." + applicationKey, GetType().Assembly);
        }

        public bool MissingResourceFile
        {
            get; private set;
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
