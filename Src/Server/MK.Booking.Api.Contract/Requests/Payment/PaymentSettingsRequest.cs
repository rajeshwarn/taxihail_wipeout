using ServiceStack.ServiceHost;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Common.Configuration.Impl;

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment
{
    [Route("/settings/payments", "GET")]
    public class UpdatePaymentSettingsRequest : IReturnVoid
    {
        public ServerPaymentSettings ServerPaymentSettings { get; set; }
    }
}
