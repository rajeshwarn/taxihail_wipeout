﻿using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment
{
    [Authenticate]
    [Route("/account/overduepayment", "GET")]
    public class OverduePaymentRequest
    {
    }
}