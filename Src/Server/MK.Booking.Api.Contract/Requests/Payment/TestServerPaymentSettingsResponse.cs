using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Booking.Api.Contract.Security;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Configuration.Impl;

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment
{

    public class TestServerPaymentSettingsResponse
    {
        public bool IsSuccessful { get; set; }
        public string Message { get; set; }
    }
}
