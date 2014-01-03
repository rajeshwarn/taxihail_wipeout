#region

using System.Dynamic;
using System.Resources;
using apcurium.MK.Common.Extensions;

#endregion

namespace apcurium.MK.Booking.Resources
{
    public class DynamicResources : DynamicObject
    {
        private readonly ResourceManager _resources;

        public DynamicResources(string applicationKey)
        {
            var names = GetType().Assembly.GetManifestResourceNames();
            var resourceName = "apcurium.MK.Booking.Resources." + applicationKey + ".resources";

            MissingResourceFile = names.None(n => n.ToLower() == resourceName.ToLower());
            // resource files doesn't exist for all the customers

            _resources = new ResourceManager("apcurium.MK.Booking.Resources." + applicationKey, GetType().Assembly);
        }

        public bool MissingResourceFile { get; private set; }

        public string GetString(string key)
        {
            if (MissingResourceFile)
            {
                return Global.ResourceManager.GetString(key);
            }
            return _resources.GetString(key)
                   ?? Global.ResourceManager.GetString(key);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = GetString(binder.Name);
            return true;
        }
    }
}