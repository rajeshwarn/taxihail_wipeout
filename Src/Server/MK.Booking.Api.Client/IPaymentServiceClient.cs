using System;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Client.Cmt.Payments.Tokenize;

namespace MK.Booking.Api.Client
{
	public interface IPaymentServiceClient
	{
		TokenizeResponse Tokenize(string creditCardNumber, DateTime expiryDate, string cvv);

		TokenizeDeleteResponse ForgetTokenizedCard(string cardToken);

        string PreAuthorize(string cardToken, string encryptedCvv, double amount, string orderNumber);

        bool CommitPreAuthorized(string transactionId, string orderNumber);
	}
}

