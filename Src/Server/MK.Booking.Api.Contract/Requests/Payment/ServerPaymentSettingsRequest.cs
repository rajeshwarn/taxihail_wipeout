#region

using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using ServiceStack.ServiceHost;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment
{
    [Route("/settings/payments/server", "GET")]
    public class ServerPaymentSettingsRequest : IReturn<ServerPaymentSettingsResponse>
    {
    }
}