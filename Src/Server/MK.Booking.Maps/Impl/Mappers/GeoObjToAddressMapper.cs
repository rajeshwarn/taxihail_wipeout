#region

using System;
using System.Linq;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Booking.MapDataProvider.Resources;

#endregion

namespace apcurium.MK.Booking.Maps.Impl.Mappers
{
    public class GeoObjToAddressMapper
    {
        public Address ConvertToAddress(GeoAddress geoAddress , string placeName, bool foundByName)
        {
            var address = new Address
            {
				FriendlyName = placeName,
                FullAddress = geoAddress.FullAddress,
                Id = Guid.NewGuid(),
                Latitude = geoAddress.Latitude,
                Longitude = geoAddress.Longitude,
                StreetNumber = geoAddress.StreetNumber,
                Street = geoAddress.Street,
                ZipCode = geoAddress.ZipCode,
                City = geoAddress.City,
                State = geoAddress.State, 

            };
                                    
            address.AddressType = "postal";
            address.StreetNumber = address.StreetNumber ?? "";

            var isRange = !foundByName && geoAddress.LocationType == "RANGE_INTERPOLATED";
            
            //indicates that the returned result reflects an approximation (usually on a road) interpolated between two precise points (such as intersections). Interpolated results are generally returned when rooftop geocodes are unavailable for a street address.

            //Mega patch for Four Twos!!! When you search for 2107 Utopia Pkwy, google returns 21-7.  
            //If the search was done by name ( foundByName == true ) we assume that it's an address not a range.  
            //If the search was done by geolocation ( foundByName == false ), we check the type returned by google.
            //If the search was done by name ( foundByName == true ), and the address has 2 component 21-7, the second component must be padded with a zero.
            //If this code causes a problem, we need to add a settings and make it active only for Four Twos ( or companies in queens )

            if ((isRange) && address.StreetNumber.Contains("-"))
            {
                var components = address.StreetNumber.Split('-');
                var firstMiddleCount = (components.Count()/2);
                address.StreetNumber = components.Select(AddLeadingZeroIfSingleNumber).Take(firstMiddleCount).JoinBy("");
            }


            if (address.StreetNumber.Contains("-") && (address.StreetNumber.Split('-').Count() == 2))
            {
                var streetNumberComponents = address.StreetNumber.Split('-');
                address.StreetNumber = AddLeadingZeroIfSingleNumber(streetNumberComponents[0]) +
                                       AddLeadingZeroIfSingleNumber(streetNumberComponents[1]);
            }
            else
            {
                address.StreetNumber = address.StreetNumber.Replace("-", "");
            }

            if (address.StreetNumber.IsNullOrEmpty() && placeName.HasValue())
            {
                address.FullAddress = placeName + ", " + address.FullAddress.ToSafeString();
            }
            else if (address.FullAddress.HasValue() &&
                     (address.FullAddress.Split(' ').Length > 0) &&
                     (address.StreetNumber.HasValue()))
            {
                address.FullAddress = address.FullAddress.Replace(address.FullAddress.Split(' ')[0],
                    address.StreetNumber);
            }

            return address;
        }

        private string AddLeadingZeroIfSingleNumber(string val)
        {
            if ((val.Length == 1) && (char.IsNumber(val[0])))
            {
                return "0" + val;
            }
            return val;
        }

        public static bool IsOdd(int value)
        {
            return value%2 != 0;
        }
    }
}