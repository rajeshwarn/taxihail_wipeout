using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Common.Http;
using apcurium.MK.Common.Http.Response;

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment
{
    [RouteDescription("/settings/payments", "GET")]
    public class PaymentSettingsRequest : IReturn<PaymentSettingsResponse>
    {
    }
}