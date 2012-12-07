using apcurium.MK.Booking.Google;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using apcurium.MK.Common.Provider;

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
        public Address[] Search(string name, double? latitude, double? longitude)
        {
			if( name == null) throw new ArgumentNullException("name");
			if( name.Length == 0) throw new ArgumentException("name should not be emtpy", "name");
            
			var term = name.Substring(0, 1);
            int n;
            var isNumeric = int.TryParse(term, out n);

            IEnumerable<Address> addressesGeocode = null;
            IEnumerable<Address> addressesPlaces = null;


            var t1 = Task.Factory.StartNew(() =>
            {
                var geoCodingService = new Geocoding(_client, _configManager, _popularAddressProvider);
                addressesGeocode = (Address[])geoCodingService.Search(name).Take(20).ToArray();
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
