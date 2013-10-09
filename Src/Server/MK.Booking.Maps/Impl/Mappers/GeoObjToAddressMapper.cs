using System;
using System.Linq;
using apcurium.MK.Booking.Google.Resources;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Maps.Impl.Mappers
{
	public class GeoObjToAddressMapper
	{
		public Address ConvertToAddress (GeoObj geoCodeResult, string placeName)
		{
			var address = new Address {
				FullAddress = geoCodeResult.Formatted_address,
				Id = Guid.NewGuid(),
				Latitude = geoCodeResult.Geometry.Location.Lat,
				Longitude = geoCodeResult.Geometry.Location.Lng
			};

			geoCodeResult.Address_components.FirstOrDefault (x => x.AddressComponentTypes.Any (t => t == AddressComponentType.Street_number)).Maybe (x => address.StreetNumber = x.Long_name);
			var component = (from c in geoCodeResult.Address_components
                             where (c.AddressComponentTypes.Any (x => x == AddressComponentType.Route || x == AddressComponentType.Street_address) && !string.IsNullOrEmpty (c.Long_name))
                             select c).FirstOrDefault ();
			component.Maybe (c => address.Street = c.Long_name);
			geoCodeResult.Address_components.FirstOrDefault (x => x.AddressComponentTypes.Any (t => t == AddressComponentType.Postal_code)).Maybe (x => address.ZipCode = x.Long_name);
			geoCodeResult.Address_components.FirstOrDefault (x => x.AddressComponentTypes.Any (t => t == AddressComponentType.Locality)).Maybe (x => address.City = x.Long_name);
			geoCodeResult.Address_components.FirstOrDefault (x => x.AddressComponentTypes.Any (t => t == AddressComponentType.Administrative_area_level_1)).Maybe (x => address.State = x.Short_name);

			address.AddressType = "postal";
			address.StreetNumber = address.StreetNumber ?? "";

			bool isRange = geoCodeResult.Geometry.Location_type == "RANGE_INTERPOLATED";//indicates that the returned result reflects an approximation (usually on a road) interpolated between two precise points (such as intersections). Interpolated results are generally returned when rooftop geocodes are unavailable for a street address.

			if ( (isRange) && address.StreetNumber.Contains ("-") )
			{
				var components = address.StreetNumber.Split ('-');
				int firstMiddleCount = (components.Count () / 2); 
				address.StreetNumber = components.Take (firstMiddleCount).JoinBy ("");
			}

			address.StreetNumber = address.StreetNumber.Replace ("-", "");

			if (address.StreetNumber.IsNullOrEmpty () && placeName.HasValue ())
			{
				address.FullAddress = placeName + ", " + address.FullAddress.ToSafeString ();
			} 
			else if (address.FullAddress.HasValue () &&
			           (address.FullAddress.Split (' ').Length > 0) &&
			         (address.StreetNumber.HasValue()))
			{
				address.FullAddress = address.FullAddress.Replace (address.FullAddress.Split (' ') [0], address.StreetNumber);
			}

			return address;
		}

		public static bool IsOdd(int value)
		{
			return value % 2 != 0;
		}

	}
}