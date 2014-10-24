using System.Xml.Serialization;

namespace CMTPayment.Tokenize
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