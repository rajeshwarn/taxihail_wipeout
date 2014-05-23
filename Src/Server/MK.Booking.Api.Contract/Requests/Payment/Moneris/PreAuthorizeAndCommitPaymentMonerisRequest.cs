using System;
using ServiceStack.ServiceHost;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment.Moneris
{
	[Authenticate]
	[Route("/payments/braintree/preAuthorizeAndCommitPayment", "POST")]
	public class PreAuthorizeAndCommitPaymentMonerisRequest : IReturn<CommitPreauthorizedPaymentResponse>
	{
		public double Amount { get; set; }
		public double MeterAmount { get; set; }
		public double TipAmount { get; set; }
		public string CardToken { get; set; }
		public Guid OrderId { get; set; }
	}
}

