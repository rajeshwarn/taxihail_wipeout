using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;

namespace apcurium.MK.Booking.Api.Client.Cmt.Payments.Tokenize
{
    [XmlRoot(Namespace="")]
    public class TokenizeResponse : PaymentResponse
    {
        [XmlElement("cardOnFileToken")]
        public string CardOnFileToken { get; set; }

        [XmlElement("cardType")]
        public string CardType{get; set;}

        [XmlElement("lastFour")]
        public string LastFour{get; set;}        

    }
}
