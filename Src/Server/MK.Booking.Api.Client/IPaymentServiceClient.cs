using System;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;

namespace apcurium.MK.Booking.Api.Client
{
	public interface IPaymentServiceClient
	{
        void ResendConfirmationToDriver(Guid orderId);
        TokenizedCreditCardResponse Tokenize(string creditCardNumber, DateTime expiryDate, string cvv);
        DeleteTokenizedCreditcardResponse ForgetTokenizedCard(string cardToken);
        PreAuthorizePaymentResponse PreAuthorize(string cardToken, double amount, Guid orderId);
        CommitPreauthorizedPaymentResponse CommitPreAuthorized(string transactionId);
	}
}

