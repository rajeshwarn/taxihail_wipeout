#region

using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment
{
    [Authenticate]
    [Route("/settings/payments", "GET")]
    public class PaymentSettingsRequest : IReturn<PaymentSettingsResponse>
    {
    }
}