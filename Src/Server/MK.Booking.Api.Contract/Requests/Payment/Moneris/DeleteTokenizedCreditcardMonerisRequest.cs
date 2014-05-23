using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment.Moneris
{
	[Authenticate]
    [Route("/payments/moneris/deleteToken/{CardToken}", "DELETE")]
	public class DeleteTokenizedCreditcardMonerisRequest : IReturn<DeleteTokenizedCreditcardResponse>
	{
		public string CardToken { get; set; }
	}
}

