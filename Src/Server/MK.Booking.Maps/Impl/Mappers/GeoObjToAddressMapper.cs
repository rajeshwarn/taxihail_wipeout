using System;
using System.Linq;
using apcurium.MK.Booking.Google.Resources;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Maps.Impl.Mappers
{
    public class GeoObjToAddressMapper
    {
        public Address ConvertToAddress(GeoObj geoCodeResult, bool allowRange, string placeName=null)
        {
            var address = new Address
            {
                FullAddress = geoCodeResult.Formatted_address,
                Id = Guid.NewGuid(),
                Latitude = geoCodeResult.Geometry.Location.Lat,
                Longitude = geoCodeResult.Geometry.Location.Lng
            };

            geoCodeResult.Address_components.FirstOrDefault(x => x.AddressComponentTypes.Any(t => t == AddressComponentType.Street_number)).Maybe(x => address.StreetNumber = x.Long_name);
            var component = (from c in geoCodeResult.Address_components
                             where (c.AddressComponentTypes.Any(x => x == AddressComponentType.Route || x == AddressComponentType.Street_address) && !string.IsNullOrEmpty(c.Long_name))
                             select c).FirstOrDefault();
            component.Maybe(c => address.Street = c.Long_name);
            geoCodeResult.Address_components.FirstOrDefault(x => x.AddressComponentTypes.Any(t => t == AddressComponentType.Postal_code)).Maybe(x => address.ZipCode = x.Long_name);
            geoCodeResult.Address_components.FirstOrDefault(x => x.AddressComponentTypes.Any(t => t == AddressComponentType.Locality)).Maybe(x => address.City = x.Long_name);
            geoCodeResult.Address_components.FirstOrDefault(x => x.AddressComponentTypes.Any(t => t == AddressComponentType.Administrative_area_level_1)).Maybe(x => address.State = x.Short_name);

            address.AddressType = "postal";

            if (!allowRange &&
                address.StreetNumber.HasValue() &&
                address.StreetNumber.Contains("-"))
            {
                address.StreetNumber = address.StreetNumber.Split('-')[0].Trim();
            }

            if ( address.StreetNumber.IsNullOrEmpty() && placeName.HasValue())
            {
                address.FullAddress = placeName +", " + address.FullAddress.ToSafeString();
            }
            else if (address.FullAddress.HasValue() &&
                address.FullAddress.Contains("-"))
            {
                var firstWordStreetNumber = address.FullAddress.Split(' ')[0];
                if (firstWordStreetNumber.Contains("-"))
                {
                    var newStreetNUmber = firstWordStreetNumber.Split('-')[0].Trim();
                    address.FullAddress = address.FullAddress.Replace(firstWordStreetNumber, newStreetNUmber);
                }
            }

            return address;
        }
    }
}
