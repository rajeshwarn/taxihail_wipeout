using apcurium.MK.Common.Configuration.Impl;

namespace apcurium.MK.Booking.Mobile.Infrastructure
{
    public interface IPayPalConfigurationService
    {
        void InitializeService(PayPalClientSettings payPalSettings);
        object GetConfiguration();
    }
}