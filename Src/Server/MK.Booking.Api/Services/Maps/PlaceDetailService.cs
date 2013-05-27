using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Maps;
using apcurium.MK.Common.Entity;


namespace apcurium.MK.Booking.Api.Services
{
    public class PlaceDetailService : Service
    {
        private IPlaces _client;        

        public PlaceDetailService(IPlaces client)
        {
            _client = client;
        }


        public Address Get(PlaceDetailRequest request)
        {

            return _client.GetPlaceDetail(request.PlaceName, request.ReferenceId);
            

        }

    }
}
