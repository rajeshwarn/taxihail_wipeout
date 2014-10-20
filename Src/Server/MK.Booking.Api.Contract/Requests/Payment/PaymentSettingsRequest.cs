﻿using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment
{
    [Route("/settings/payments", "GET")]
    public class PaymentSettingsRequest : IReturn<PaymentSettingsResponse>
    {
    }
}