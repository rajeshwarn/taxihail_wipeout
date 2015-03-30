using apcurium.MK.Booking.Services.Impl;

namespace apcurium.MK.Booking.Services
{
    public interface IPayPalServiceFactory
    {
        PayPalService GetInstance();
    }
}
