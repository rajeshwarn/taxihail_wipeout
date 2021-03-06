﻿using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/roaming/market", "GET")]
    public class FindMarketRequest : BaseDto
    {
        public double Latitude { get; set; }

        public double Longitude { get; set; }
    }
}