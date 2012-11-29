using System;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [RestService("/account/paymentprofile", "POST")]
    public class UpdatePaymentProfileRequest
    {
        public Guid? DefaultCreditCard { get; set; }
        public double? DefaultTipAmount { get; set; }
        public double? DefaultTipPercent { get; set; } 
    }
}