#region

using apcurium.MK.Booking.Api.Client.Cmt.Payments;

#endregion

namespace apcurium.MK.Booking.Api.Client.Payments.CmtPayments.Tokenize
{
    [XmlRoot(Namespace = "")]
    public class TokenizeResponse : PaymentResponse
    {
        [XmlElement("cardOnFileToken")]
        public string CardOnFileToken { get; set; }

        [XmlElement("cardType")]
        public string CardType { get; set; }

        [XmlElement("lastFour")]
        public string LastFour { get; set; }
    }
}