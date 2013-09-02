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
        readonly bool _missingResourceFile;

        public DynamicResources(string applicationKey)
        {
            _resources = new ResourceManager("apcurium.MK.Booking.Resources." + applicationKey, GetType().Assembly);
            _missingResourceFile = false;
            
            try
            {
                _resources.GetString("test");
            }
            catch (System.Resources.MissingManifestResourceException)
            {
                // It's all right, we'll use the Global resource file instead
                _missingResourceFile = true;
            }
        }

        public override bool TryGetMember(System.Dynamic.GetMemberBinder binder, out object result)
        {
            if (_missingResourceFile)
            {
                result = Global.ResourceManager.GetString(binder.Name);
            }
            else
            {
                result = _resources.GetString(binder.Name)
                    ?? Global.ResourceManager.GetString(binder.Name);
            }
            return true;
        } 


    }
}
