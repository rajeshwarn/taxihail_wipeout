using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Google;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Services.Mappers;

namespace apcurium.MK.Booking.Api.Services
{
    public class PlaceDetailService : RestServiceBase<PlaceDetailRequest>
    {
        private IMapsApiClient _client;
        private readonly IConfigurationManager _configurationManager;

        public PlaceDetailService(IMapsApiClient client, IConfigurationManager configurationManager)
        {
            _client = client;
            _configurationManager = configurationManager;
        }


        public override object OnGet(PlaceDetailRequest request)
        {
        
            var place = _client.GetPlaceDetail( request.ReferenceId );

            var result = new GeoObjToAddressMapper().ConvertToAddress(place);
            
            result.PlaceReference = request.ReferenceId;

            return result;
            

        }

    }
}
