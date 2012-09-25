using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Google;
using apcurium.MK.Common.Configuration;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Api.Services
{
    public class SearchLocationsService : RestServiceBase<SearchLocationsRequest>
    {
        private readonly IMapsApiClient _client;
        private IConfigurationManager _configManager;

        public SearchLocationsService(IMapsApiClient client, IConfigurationManager configurationManager)
        {
            _client = client;
            _configManager = configurationManager;
        }

        public override object OnGet(SearchLocationsRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Name))
            {
                throw new HttpError(HttpStatusCode.BadRequest, ErrorCode.Search_Locations_NameRequired.ToString());
            }

            var term = request.Name.Substring(0,1);
            int n;
            var isNumeric = int.TryParse(term, out n);

            IEnumerable<Address> addresses = null;
            if (isNumeric)
            {
                var geoCodingService = new GeocodingService(_client, _configManager);
                var list =  (AddressList)geoCodingService.OnGet(new GeocodingRequest { Name = request.Name });
                addresses = list.Addresses;
            }
            else
            {
                var nearbyService = new NearbyPlacesService(_client, _configManager);
                addresses = (Address[])nearbyService.OnGet(new NearbyPlacesRequest { Name = request.Name, Lat = request.Lat, Lng = request.Lng });                
            }

            return new AddressList { Addresses = addresses.Take(5).ToArray() };
        }
    }
}
