using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment
{
	[Authenticate]
	[Route("/settings/encryptedpayments", "GET")]
	public class EncryptedPaymentSettingsRequest
	{
	}
}
