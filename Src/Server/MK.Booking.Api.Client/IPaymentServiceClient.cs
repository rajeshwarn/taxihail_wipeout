using System;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Client.Cmt.Payments.Tokenize;

namespace MK.Booking.Api.Client
{
	public interface IPaymentServiceClient
	{
		TokenizedCreditCardResponse Tokenize(string creditCardNumber, DateTime expiryDate, string cvv);

        DeleteTokenizedCreditcardResponse ForgetTokenizedCard(string cardToken);

        PreAuthorizePaymentResponse PreAuthorize(string cardToken, double amount, string orderNumber);

        CommitPreauthoriedPaymentResponse CommitPreAuthorized(string transactionId, string orderNumber);
	}

    public class BasePaymentResponse
    {
        public bool IsSuccessfull { get; set; }
        
        public string Message{ get; set; }
        
    }
    public class CommitPreauthoriedPaymentResponse : BasePaymentResponse
    {
    }

    public class PreAuthorizePaymentResponse : BasePaymentResponse
    {
        public string TransactionId { get; set; }

   
    }

    public class DeleteTokenizedCreditcardResponse : BasePaymentResponse
    {

    }

    public class TokenizedCreditCardResponse : BasePaymentResponse
    {
        public string LastFour { get; set; }
        public string CardType { get; set; }

        public string CardOnFileToken { get; set; }
    }
}

