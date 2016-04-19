#region

using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Http;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment
{
    [RouteDescription("/settings/payments/server", "POST")]
    public class UpdateServerPaymentSettingsRequest
    {
        public ServerPaymentSettings ServerPaymentSettings { get; set; }
    }
}