using System;
using System.Linq;
using System.Net;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Booking.Maps;

namespace apcurium.MK.Booking.Api.Services
{
    public class NearbyPlacesService : RestServiceBase<NearbyPlacesRequest>
    {
        private IPlaces _client;
        private readonly IConfigurationManager _configurationManager;

        public NearbyPlacesService(IPlaces client, IConfigurationManager configurationManager)
        {
            _client = client;
            _configurationManager = configurationManager;
        }

        public override object OnGet(NearbyPlacesRequest request)
        {
            if (string.IsNullOrEmpty(request.Name) 
                && request.IsLocationEmpty())
            {
                throw new HttpError(HttpStatusCode.BadRequest, ErrorCode.NearbyPlaces_LocationRequired.ToString());
            }

            return _client.SearchPlaces(request.Name, request.Lat, request.Lng, request.Radius);
           
        }

     

    }
}
