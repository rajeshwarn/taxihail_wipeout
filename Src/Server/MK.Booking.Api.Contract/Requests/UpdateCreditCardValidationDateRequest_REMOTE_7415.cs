using System;
using apcurium.MK.Common.Resources;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [Route("/account/creditcard/updatevalidationdate", "POST")]
    public class UpdateCreditCardValidationDateRequest : IReturn<BasePaymentResponse>
    {
        public Guid CreditCardId { get; set; }
        public DateTime? LastTokenValidateDateTime { get; set; }

    }
}
