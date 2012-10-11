using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using ServiceStack.ServiceClient.Web;
using apcurium.MK.Common.Extensions;
using ServiceStack.Common.Web;
using System.Net;
using System.Globalization;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Booking.Maps;

namespace apcurium.MK.Booking.Api.Services
{



    public class DirectionsService : RestServiceBase<DirectionsRequest>
    {
        private readonly IDirections _client;
        
        public DirectionsService(IDirections client)
        {
            _client = client;
        }


        public override object OnGet(DirectionsRequest request)
        {
            var result = _client.GetDirection(request.OriginLat, request.OriginLng, request.DestinationLat, request.DestinationLng);
            return new DirectionInfo { Distance = result.Distance, FormattedDistance = result.FormattedDistance, FormattedPrice = result.FormattedPrice, Price = result.Price };          
        }

      

    }
}
