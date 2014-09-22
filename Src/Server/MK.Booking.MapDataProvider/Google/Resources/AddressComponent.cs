#region

using apcurium.MK.Common.Extensions;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace apcurium.MK.Booking.MapDataProvider.Google.Resources	
{
    public class AddressComponent
    {
        public string Long_name { get; set; }
        public string Short_name { get; set; }

        public List<string> Types { get; set; }

        public IEnumerable<AddressComponentType> AddressComponentTypes
        {
            get { return Types.Select(type => type.ToEnum(true, AddressComponentType.Unkown)); }
        }
    }
}