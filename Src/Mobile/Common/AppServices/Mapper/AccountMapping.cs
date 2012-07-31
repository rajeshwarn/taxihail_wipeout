//using System;
//using System.Linq;
//using apcurium.Framework;
//using apcurium.Framework.Extensions;
//using TinyIoC;


//namespace apcurium.MK.Booking.Mobile
//{
//    public class AccountMapping
//    {
//        public AccountMapping ()
//        {
//        }

//        public IBS.AccountInfo ToWSData (IBS.AccountInfo result, Account data)
//        {
//            result.Addresses = data.FavoriteLocations.Select (loc => ToWSLocationData (loc)).ToArray ();
//            return result;
			
//        }


//        public Account ToData (Account existing, IBS.AccountInfo account)
//        {
//            Account result;
//            if ( existing != null )
//            {
//                result = existing;
//            }
//            else 
//            {
//                 result = new Account ();
//            }
			
					
			
//            result.Name = Params.Get (account.FirstName, account.LastName).Where (s => s.HasValue ()).JoinBy (" ");
//            result.Email = account.Email;						
			
//            if ( account.Addresses != null )
//            {
//                result.FavoriteLocations = account.Addresses.Where( a => a.AddressAndCity.HasValue()) .Select (a => ToLocationData (existing, a)).ToArray ();
//            }
//            return result;
//        }


//        public IBS.AccountAddress ToWSLocationData (LocationData location)
//        {
//            var result = new IBS.AccountAddress ();
//            result.Id = location.Id;
//            result.LocationName = location.Name;
//            result.AddressAndCity = location.Address;
//            result.Appartment = location.Apartment;
//            result.Latitude = location.Latitude.HasValue ? location.Latitude.Value : 0;
//            result.Longitude = location.Longitude.HasValue ? location.Longitude.Value : 0;
//            return result;
//        }
//        public LocationData ToLocationData (Account existing, IBS.AccountAddress address)
//        {
//            var result = new LocationData ();
//            result.Id = address.Id;
//            result.Name = address.LocationName;
			
//            if ((existing != null) && (address.Id > 0)) {
//                var r = existing.FavoriteLocations.FirstOrDefault (l => l.Id == address.Id);
//                if (r != null) {
//                    result = r;
//                }
//            }
			
//            UpdateLocationData( result , address );
			
			
			
//            return result;
//        }
		
		
//        public void UpdateLocationData( LocationData result , IBS.AccountAddress address) 
//        {
//            if ( address.Id == result.Id  && address.Latitude != 0  && address.Longitude != 0 )
//            {
//                result.Longitude = address.Longitude;
//                result.Latitude = address.Latitude;				
//            }
//            else if ( address.LocationName.HasValue() && (address.AddressAndCity.Trim ().ToLower () != result.Address.SelectOrDefault (s => s, "").Trim ().ToLower ()) ){
//                result.Latitude = null;
//                result.Longitude = null;
//            }
			
//            if (address.LocationName.HasValue() &&((!result.Longitude.HasValue) || (result.Longitude.Value == 0) || (!result.Latitude.HasValue) || (result.Latitude.Value == 0))) {
//                var adrs = TinyIoCContainer.Current.Resolve<IBookingService>().SearchAddress(address.AddressAndCity).FirstOrDefault();
//                if (adrs != null) {
//                    result.Latitude = adrs.Latitude;
//                    result.Longitude = adrs.Longitude;
//                }
//            }
			
//            result.Id = address.Id;
//            result.Name = address.LocationName;
//            result.Address = address.AddressAndCity;
//            result.Apartment = address.Appartment;
//        }
//    }
//}

