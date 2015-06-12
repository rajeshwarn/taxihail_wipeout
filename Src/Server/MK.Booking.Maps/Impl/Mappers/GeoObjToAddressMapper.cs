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
        public Address ConvertToAddress(GeoAddress geoAddress, string placeName, bool foundByName)
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
            
            //If the search was done by geolocation ( foundByName == false ), we check the type returned by google to see if it's a range.
            if (isRange && address.StreetNumber.Contains("-"))
            {
                var components = address.StreetNumber.Split('-');
                var firstMiddleCount = (components.Count()/2);

                var newStreetNumber = components.Select(AddLeadingZeroIfSingleNumber).Take(firstMiddleCount).JoinBy("");
                ChangeStreetNumber(address, newStreetNumber);
            }

            if (address.StreetNumber.IsNullOrEmpty() && placeName.HasValue())
            {
                address.FullAddress = placeName + ", " + address.FullAddress.ToSafeString();
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

        private void ChangeStreetNumber(Address address, string newStreetNumber)
        {
            if (address.StreetNumber.HasValue() && newStreetNumber.HasValue())
            {
                address.FullAddress = address.FullAddress.Replace(address.StreetNumber, newStreetNumber);
            }
            address.StreetNumber = newStreetNumber;
        }
        
        public static bool IsOdd(int value)
        {
            return value%2 != 0;
        }
    }
}