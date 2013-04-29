using System;
using apcurium.MK.Booking.Api.Client;

namespace MK.Booking.Api.Client
{
	public interface IPaymentClient : ICreditCardTokenizationService
	{
        long PreAuthorize(string cardToken, double amount, string orderNumber);

        bool CommitPreAuthorized(long transactionId, string orderNumber);
	}
}

