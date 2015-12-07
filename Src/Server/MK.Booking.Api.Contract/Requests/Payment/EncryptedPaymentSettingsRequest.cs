using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment
{
	[Authenticate]
	[Route("/encryptedsettings/payments", "GET")]
	public class EncryptedPaymentSettingsRequest
	{
	}
}
