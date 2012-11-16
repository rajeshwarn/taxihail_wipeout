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

        public Address[] Search(string name, double latitude, double longitude)
        {
            var term = name.Substring(0, 1);

            int n;
            var isNumeric = int.TryParse(term, out n);

            IEnumerable<Address> addresses = null;
            if (isNumeric)
            {

                var geoCodingService = new Geocoding(_client, _configManager, _popularAddressProvider);
                var list = (Address[])geoCodingService.Search(name).Take(5).ToArray();
                var listPopular = _popularAddressProvider.GetPopularAddresses().Where(c=>c.FullAddress.Contains(name)).ToList();
                foreach (var address in list)
                {
                    listPopular.Add(address);
                }
                addresses = listPopular;
            }
            else
            {
                var nearbyService = new Places(_client, _configManager, _popularAddressProvider);
                addresses = (Address[])nearbyService.SearchPlaces( name, latitude,longitude, null );
            }

            return addresses.ToArray();
        }

    }
}
