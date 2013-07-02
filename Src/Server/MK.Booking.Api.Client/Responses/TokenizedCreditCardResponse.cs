namespace apcurium.MK.Booking.Api.Client.Responses
{
    public class TokenizedCreditCardResponse : BasePaymentResponse
    {
        public string LastFour { get; set; }
        public string CardType { get; set; }

        public string CardOnFileToken { get; set; }
    }
}