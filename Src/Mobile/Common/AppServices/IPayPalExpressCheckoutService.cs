using System;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.AppServices
{
    public interface IPayPalExpressCheckoutService
    {
        Task<string> SetExpressCheckoutForAmount(decimal amount);

    }
}

