#region

using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Extensions;

#endregion

namespace apcurium.MK.Booking.Google.Resources
{
    public class PlaceTypes
    {
        public PlaceTypes(IConfigurationManager configManager)
        {
            var types = configManager.GetSetting("GeoLoc.PlacesTypes");

            Types = types.Split(',').Where(s => s.ToSafeString().Trim().HasValue());
        }

        public IEnumerable<string> Types { get; set; }

        public string GetPipedTypeList()
        {
            return string.Join("|", Types);
        }
    }
}