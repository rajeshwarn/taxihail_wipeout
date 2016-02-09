using apcurium.MK.Booking.Api.Contract.Resources;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
	[Authenticate]
	[Route("/account/creditcardinfo/{CreditCardId}", "GET")]
	public class CreditCardInfoRequest : IReturn<CreditCardDetails>
	{
		public Guid CreditCardId { get; set; }
	}
}
