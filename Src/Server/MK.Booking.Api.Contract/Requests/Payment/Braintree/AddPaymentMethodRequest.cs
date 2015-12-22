using System;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceHost;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment.Braintree
{
	[Authenticate]
	[Route("/payments/braintree/addpaymentmethod", "POST")]
	public class AddPaymentMethodRequest : IReturn<TokenizedCreditCardResponse>
	{
		public string Nonce	{ get; set; }
	    public PaymentMethods PaymentMethod { get; set; }
	}
}

