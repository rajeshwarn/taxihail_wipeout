using ServiceStack.ServiceHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;

namespace apcurium.MK.Booking.Api.Client.Cmt.Payments.Tokenize
{
    [Route("v2/tokenize")]
    public class TokenizeRequest : IReturn<TokenizeResponse>
    {
        public TokenizeRequest()
        {
            ValidateAccountInformation = true;
        }
        public string AccountNumber {get; set;}

        public string ExpiryDate { get; set; }
        
        public bool ValidateAccountInformation { get; set; }
    }
}
