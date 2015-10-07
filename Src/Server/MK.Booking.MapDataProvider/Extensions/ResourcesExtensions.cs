using System;
using System.Threading.Tasks;
using ServiceStack.ServiceClient.Web;
using ServiceStack.ServiceHost;
using apcurium.MK.Booking.MapDataProvider.Resources;
using apcurium.MK.Booking.MapDataProvider.Google.Resources;
using System.Linq;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.MapDataProvider.Extensions
{
	public static class ResourcesExtensions
	{
		private static readonly string[] _otherTypesAllowed = {
			"airport", "transit_station", "bus_station", "train_station",
			"route", "postal_code", "street_address", "premise", "subpremise"
		};

		public static GeoAddress[] ConvertGeoResultToAddresses(this GeoResult result)
		{ 
			if (result.Status == ResultStatus.OK )
			{
				return result.Results
					.Where(r => 
						r.Formatted_address.HasValue() 
						&& r.Geometry != null 
						&& r.Geometry.Location != null 
						&& r.Geometry.Location.Lng != 0 
						&& r.Geometry.Location.Lat != 0 
						&& (r.AddressComponentTypes.Any(type => type == AddressComponentType.Street_address) 
							|| (r.Types.Any(t => _otherTypesAllowed.Any(o => o.ToLower() == t.ToLower())))))
					.Select(ConvertGeoObjectToAddress)
					.ToArray();
			}
			else
			{
				return new GeoAddress[0];
			}
		}

		public static GeoAddress ConvertGeoObjectToAddress(GeoObj geoResult)
		{        
			var address = new GeoAddress
			{
				FullAddress = geoResult.Formatted_address,                
				Latitude = geoResult.Geometry.Location.Lat,
				Longitude = geoResult.Geometry.Location.Lng
			};

			geoResult.Address_components
				.FirstOrDefault(x => x.AddressComponentTypes.Any(t => t == AddressComponentType.Street_number))
				.Maybe(x => address.StreetNumber = x.Long_name);

			var component = (from c in geoResult.Address_components
				where
				(c.AddressComponentTypes.Any(
					x => x == AddressComponentType.Route || x == AddressComponentType.Street_address) && 
					!string.IsNullOrEmpty(c.Long_name))
				select c).FirstOrDefault();
			component.Maybe(c => address.Street = c.Long_name);

			geoResult.Address_components
				.FirstOrDefault(x => x.AddressComponentTypes.Any(t => t == AddressComponentType.Postal_code))
				.Maybe(x => address.ZipCode = x.Long_name);

			geoResult.Address_components
				.FirstOrDefault(x => x.AddressComponentTypes.Any(t => t == AddressComponentType.Locality))
				.Maybe(x => address.City = x.Long_name);

			if (address.City == null)
			{
				// some times, the city is not set by Locality, for example in Brooklyn and Queens
				geoResult.Address_components
					.FirstOrDefault(x => x.AddressComponentTypes.Any(t => t == AddressComponentType.Sublocality))
					.Maybe(x => address.City = x.Long_name);
			}

			geoResult.Address_components
				.FirstOrDefault(x => x.AddressComponentTypes.Any(t => t == AddressComponentType.Administrative_area_level_1))
				.Maybe(x => address.State = x.Short_name);

			address.LocationType = geoResult.Geometry.Location_type;

			return address;
		}
	}
}