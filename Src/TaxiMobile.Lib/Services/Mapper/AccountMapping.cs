using System.Linq;
using TaxiMobile.Lib.Data;
using TaxiMobile.Lib.Framework;
using TaxiMobile.Lib.Framework.Extensions;
using TaxiMobile.Lib.Practices;
using TaxiMobile.Lib.Services.IBS;

namespace TaxiMobile.Lib.Services.Mapper
{
	public class AccountMapping
	{
        public TBookAccount3 ToWSData(TBookAccount3 result, AccountData data)
		{
			result.FavoriteAddresses = data.FavoriteLocations.Select (loc => ToWSLocationData (loc)).ToArray ();
			return result;
			
		}


		public AccountData ToData (AccountData existing, TBookAccount3 account)
		{
		    var result = existing ?? new AccountData ();
			
			result.Name = Params.Get (account.FirstName, account.LastName).Where (s => s.HasValue ()).JoinBy (" ");
            result.Email = account.eMail;						
			
			if ( account.FavoriteAddresses != null )
			{
                result.FavoriteLocations = account.FavoriteAddresses.Select(a => ToLocationData(existing, a)).ToArray();
			}
			return result;
		}


		public TWEBFavotiteAddress ToWSLocationData (LocationData location)
		{
            var result = new TWEBFavotiteAddress();
			result.AddressID = location.Id;
			result.AddressAlias = location.Name;
			result.StreetPlace = location.Address;
			result.AptBaz = location.Apartment;
			result.Latitude = location.Latitude.HasValue ? location.Latitude.Value : 0;
			result.Longitude = location.Longitude.HasValue ? location.Longitude.Value : 0;
			return result;
		}
		public LocationData ToLocationData (AccountData existing, TWEBFavotiteAddress address)
		{
			var result = new LocationData ();
			result.Id = address.AddressID;
			result.Name = address.AddressAlias;
			
			if ((existing != null) && (address.AddressID > 0)) {
				var r = existing.FavoriteLocations.FirstOrDefault (l => l.Id == address.AddressID);
				if (r != null) {
					result = r;
				}
			}
			
			UpdateLocationData( result , address );
			return result;
		}
		
		
		public void UpdateLocationData( LocationData result , TWEBFavotiteAddress address) 
		{
			if ( address.AddressID == result.Id  && address.Latitude != 0  && address.Longitude != 0 )
			{
				result.Longitude = address.Longitude;
				result.Latitude = address.Latitude;				
			}
			else if ( address.AddressAlias.HasValue() && (address.StreetPlace.Trim ().ToLower () != result.Address.SelectOrDefault (s => s, "").Trim ().ToLower ()) ){
				result.Latitude = null;
				result.Longitude = null;
			}
			
			if (address.AddressAlias.HasValue() &&((!result.Longitude.HasValue) || (result.Longitude.Value == 0) || (!result.Latitude.HasValue) || (result.Latitude.Value == 0))) {
                var adrs = ServiceLocator.Current.GetInstance<IBookingService>().SearchAddress(address.StreetPlace).FirstOrDefault();
				if (adrs != null) {
					result.Latitude = adrs.Latitude;
					result.Longitude = adrs.Longitude;
				}
			}
			
			result.Id = address.AddressID;
			result.Name = address.AddressAlias;
            result.Address = address.StreetPlace;
			result.Apartment = address.AptBaz;
		}
	}
}

