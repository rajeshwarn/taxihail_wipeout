using System;
using apcurium.MK.Booking.Api.Client.Responses;

namespace apcurium.MK.Booking.Api.Client
{
	public interface IPaymentServiceClient
	{

        TokenizedCreditCardResponse Tokenize(string creditCardNumber, DateTime expiryDate, string cvv);

        DeleteTokenizedCreditcardResponse ForgetTokenizedCard(string cardToken);

        PreAuthorizePaymentResponse PreAuthorize(string cardToken, double amount, string orderNumber);

        CommitPreauthoriedPaymentResponse CommitPreAuthorized(string transactionId, string orderNumber);
	}
}

