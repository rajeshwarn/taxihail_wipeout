using ServiceStack.ServiceHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;

namespace apcurium.MK.Booking.Api.Client.Cmt.Payments.Tokenize
{
    [Route("tokenize")]
    [XmlRoot(Namespace = "")]
    public class TokenizeRequest : IReturn<TokenizeResponse>
    {
        [XmlElement("accountNumber")]
        public string AccountNumber {get; set;}

        [XmlElement("expiryDate")]
        public string ExpiryDateYYMM { get; set; }
    }
}
