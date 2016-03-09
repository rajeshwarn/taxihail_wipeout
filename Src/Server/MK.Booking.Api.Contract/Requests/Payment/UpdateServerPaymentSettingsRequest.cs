#region

using apcurium.MK.Common.Configuration.Impl;
using ServiceStack.ServiceHost;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment
{
    [Route("/settings/payments/server", "POST")]
    public class UpdateServerPaymentSettingsRequest
    {
        public ServerPaymentSettings ServerPaymentSettings { get; set; }
    }
}