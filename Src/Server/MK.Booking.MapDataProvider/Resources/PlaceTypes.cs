using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.MapDataProvider.Resources
{
    public class PlaceTypes
    {
        public PlaceTypes(string placeTypes)
        {
            Types = placeTypes.Split(',').Where(s => s.ToSafeString().Trim().HasValue());
        }

        public IEnumerable<string> Types { get; set; }

        public string GetPipedTypeList()
        {
            return string.Join("|", Types);
        }
    }
}