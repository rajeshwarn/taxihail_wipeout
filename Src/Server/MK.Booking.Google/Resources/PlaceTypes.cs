using System.Linq;
using MK.Common.Android.Configuration;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Extensions;
using System.Collections.Generic;

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