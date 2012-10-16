using apcurium.MK.Booking.Google;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Maps.Impl
{
    public class Addresses : IAddresses
    {
        private readonly IMapsApiClient _client;
        private IConfigurationManager _configManager;

        public Addresses(IMapsApiClient client, IConfigurationManager configurationManager)
        {
            _client = client;
            _configManager = configurationManager;
        }

        public Address[] Search(string name, double latitude, double longitude)
        {
            var term = name.Substring(0, 1);

            int n;
            var isNumeric = int.TryParse(term, out n);

            IEnumerable<Address> addresses = null;
            if (isNumeric)
            {

                var geoCodingService = new Geocoding(_client, _configManager);
                var list = (Address[])geoCodingService.Search(name);
                addresses = list;
            }
            else
            {
                var nearbyService = new Places(_client, _configManager);
                addresses = (Address[])nearbyService.SearchPlaces( name, latitude,longitude, null ); // .OnGet(new NearbyPlacesRequest { Name = request.Name, Lat = request.Lat, Lng = request.Lng });
            }

            return addresses.Take(5).ToArray();
        }

    }
}
