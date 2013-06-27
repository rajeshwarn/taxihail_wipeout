using System;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Client.Cmt.Payments.Tokenize;

namespace MK.Booking.Api.Client
{
	public interface IPaymentServiceClient
	{
		TokenizeResponse Tokenize(string creditCardNumber, DateTime expiryDate);

		TokenizeDeleteResponse ForgetTokenizedCard(string cardToken);

        long PreAuthorize(string cardToken, double amount, string orderNumber);

        bool CommitPreAuthorized(long transactionId, string orderNumber);
	}
}

