﻿using System;
using apcurium.MK.Common.Entity;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests.Client
{
    [Authenticate]
    [Route("/client/hail/{OrderId}", "POST")]
    public class ConfirmHailRequest
    {
        public Guid TaxiHailOrderId { get; set; }

        public OrderKey OrderKey { get; set; }

        public VehicleCandidate VehicleCandidate { get; set; }
    }
}
