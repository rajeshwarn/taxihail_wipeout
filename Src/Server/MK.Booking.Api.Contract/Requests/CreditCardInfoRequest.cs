using apcurium.MK.Booking.Api.Contract.Resources;
using System;
using apcurium.MK.Common.Http;
using apcurium.MK.Common.Http.Response;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
	[RouteDescription("/account/creditcardinfo/{CreditCardId}", "GET")]
	public class CreditCardInfoRequest : IReturn<CreditCardDetails>
	{
		public Guid CreditCardId { get; set; }
	}
}
