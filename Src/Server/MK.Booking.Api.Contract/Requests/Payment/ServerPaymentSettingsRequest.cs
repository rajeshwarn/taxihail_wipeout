#region

using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Common.Http;
using apcurium.MK.Common.Http.Response;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment
{
    [RouteDescription("/settings/payments/server", "GET")]
    public class ServerPaymentSettingsRequest : IReturn<ServerPaymentSettingsResponse>
    {
    }
}