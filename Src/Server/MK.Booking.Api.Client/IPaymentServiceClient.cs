using System;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;

namespace apcurium.MK.Booking.Api.Client
{
	public interface IPaymentServiceClient
	{
        void ResendConfirmationToDriver(Guid orderId);
        TokenizedCreditCardResponse Tokenize(string creditCardNumber, DateTime expiryDate, string cvv);
        DeleteTokenizedCreditcardResponse ForgetTokenizedCard(string cardToken);
        PreAuthorizePaymentResponse PreAuthorize(string cardToken, double amount, double meterAmount, double tipAmount, Guid orderId);
        CommitPreauthorizedPaymentResponse CommitPreAuthorized(string transactionId);
        CommitPreauthorizedPaymentResponse PreAuthorizeAndCommit(string cardToken, double amount, double meterAmount, double tipAmount, Guid orderId);
        PairingResponse Pair(string medallion, string driverId, string customerId, string customerName, double latitude, double longitude, bool autoCompletePayment, int? autoTipPercentage, double? autoTipAmount);
	}
}

