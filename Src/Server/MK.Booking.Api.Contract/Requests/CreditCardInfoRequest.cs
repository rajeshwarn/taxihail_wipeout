using apcurium.MK.Booking.Api.Contract.Resources;
using ServiceStack.ServiceHost;
using System;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
	[Route("/account/creditcardinfo/{CreditCardId}", "GET")]
	public class CreditCardInfoRequest : IReturn<CreditCardDetails>
	{
		public Guid CreditCardId { get; set; }
	}
}
