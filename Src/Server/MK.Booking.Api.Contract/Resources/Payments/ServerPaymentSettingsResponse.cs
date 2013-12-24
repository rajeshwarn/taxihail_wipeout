#region

using apcurium.MK.Booking.Api.Contract.Http;
using apcurium.MK.Common.Configuration.Impl;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Resources.Payments
{
    [NoCache]
    public class ServerPaymentSettingsResponse
    {
        public ServerPaymentSettings ServerPaymentSettings { get; set; }
    }
}