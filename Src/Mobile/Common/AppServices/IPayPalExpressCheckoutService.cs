using System;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.AppServices
{
    public interface IPayPalExpressCheckoutService
    {
		Task<string> SetExpressCheckoutForAmount(Guid orderId, decimal amount, decimal meterAmout,decimal tipAmount, int? ibsOrderId, string totalAmount, string languageCode);

    }
}

