﻿using System;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [Route("/account/creditcard/updatedefault", "POST")]
    public class DefaultCreditCardRequest
    {
        public Guid CreditCardId { get; set; }
    }
}
