using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Configuration;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Maps;

namespace apcurium.MK.Booking.Api.Services
{
    public class SearchLocationsService : RestServiceBase<SearchLocationsRequest>
    {
        private readonly IAddresses _client;
        private IConfigurationManager _configManager;

        public SearchLocationsService(IAddresses client, IConfigurationManager configurationManager)
        {
            _client = client;
            _configManager = configurationManager;
        }

        public override object OnPost(SearchLocationsRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Name))
            {
                throw new HttpError(HttpStatusCode.BadRequest, ErrorCode.Search_Locations_NameRequired.ToString());
            }
            return _client.Search(request.Name, request.Lat.GetValueOrDefault(), request.Lng.GetValueOrDefault(), request.GeoResult);
           
        }
    }
}
