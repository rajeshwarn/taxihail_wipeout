using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.Infrastructure
{
	public interface IPaymentProviderClientService
	{
	    Task<string> GetPayPalNonce(string clientToken);
	    Task<string> GetCreditCardNonce(string clientToken, string creditCardNumber, string ccv, string expirationMonth, string expirationYear, string firstName, string lastName, string zipCode);
	    Task<string> GetPlatformPayNonce(string clientToken);
	}
}

