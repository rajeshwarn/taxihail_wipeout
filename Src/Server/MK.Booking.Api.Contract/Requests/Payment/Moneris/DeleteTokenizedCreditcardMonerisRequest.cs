using System;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment.Moneris
{
	[Authenticate]
	[Route("/payments/braintree/deleteToken/{CardToken}", "DELETE")]
	public class DeleteTokenizedCreditcardMonerisRequest : IReturn<DeleteTokenizedCreditcardResponse>
	{
		public string CardToken { get; set; }
	}
}

