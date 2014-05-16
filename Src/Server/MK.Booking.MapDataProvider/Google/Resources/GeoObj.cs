#region

using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Common.Extensions;

#endregion

namespace apcurium.MK.Booking.MapDataProvider.Google.Resources	
{
    public class GeoObj
    {
        public List<AddressComponent> Address_components { get; set; }
        public string Formatted_address { get; set; }
        public Geometry Geometry { get; set; }

        public List<string> Types { get; set; }

        public IEnumerable<AddressComponentType> AddressComponentTypes
        {
            get { return Types.Select(type => type.ToEnum(true, AddressComponentType.Unkown)); }
        }
    }
}