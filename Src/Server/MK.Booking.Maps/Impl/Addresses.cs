using apcurium.MK.Booking.Google;
using apcurium.MK.Booking.Google.Resources;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using apcurium.MK.Common.Provider;
using apcurium.MK.Booking.Maps.Geo;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Maps.Impl
{
    public class Addresses : IAddresses
    {
        private readonly IMapsApiClient _client;
        private IConfigurationManager _configManager;
        private readonly IPopularAddressProvider _popularAddressProvider;

        public Addresses(IMapsApiClient client, IConfigurationManager configurationManager, IPopularAddressProvider popularAddressProvider )
        {
            _client = client;
            _configManager = configurationManager;
            _popularAddressProvider = popularAddressProvider;
        }

		/// <summary>
		/// Search addresses for the specified name, latitude and longitude.
		/// </summary>
		/// <param name='name'>
		/// Search criteria, address fragment. Cannot be null or empty
		/// </param>
		/// <param name='latitude'>
		/// Latitude
		/// </param>
		/// <param name='longitude'>
		/// Longitude
		/// </param>
        public Address[] Search(string name, double? latitude, double? longitude, GeoResult geoResult = null)
        {
            if ( name.IsNullOrEmpty() )
            {
                return new Address[0];
            }			
            
			var term = name.Substring(0, 1);
            int n;
            var isNumeric = int.TryParse(term, out n);

            IEnumerable<Address> addressesGeocode = null;
            IEnumerable<Address> addressesPlaces = null;


            var t1 = Task.Factory.StartNew(() =>
            {
                var geoCodingService = new Geocoding(_client, _configManager, _popularAddressProvider);


                var allResults = geoCodingService.Search(name, geoResult);
                if ( latitude.HasValue && longitude.HasValue && ( latitude.Value != 0 || longitude.Value != 0 )  )
                {
                    addressesGeocode = allResults.OrderBy ( adrs => Position.CalculateDistance( adrs.Latitude, adrs.Longitude, latitude.Value , longitude.Value )).Take(20).ToArray();
                }
                else
                {
                    addressesGeocode = allResults.Take (20).ToArray ();
                }

            });

            Task t2 = null;
            if  (!isNumeric)
            {
                t2 = Task.Factory.StartNew(() =>
                {
                    var nearbyService = new Places(_client, _configManager, _popularAddressProvider);
                    addressesPlaces = (Address[])nearbyService.SearchPlaces(name, latitude, longitude, null).Take(20).ToArray();
                });
            }

            t1.Wait();

            if (t2 != null)
            {
                t2.Wait();
                return addressesPlaces.Concat(addressesGeocode).ToArray();
            }
            else
            {
                return addressesGeocode.ToArray();
            }

        }

    }
}
