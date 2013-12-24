#region

using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Common.Extensions;

#endregion

namespace apcurium.MK.Booking.Google.Resources
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