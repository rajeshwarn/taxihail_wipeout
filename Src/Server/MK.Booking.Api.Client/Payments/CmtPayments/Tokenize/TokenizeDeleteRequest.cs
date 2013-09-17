using ServiceStack.ServiceHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace apcurium.MK.Booking.Api.Client.Cmt.Payments.Tokenize
{
    [Route("v2/tokenize/{CardToken}")]
    public class TokenizeDeleteRequest : IReturn<TokenizeDeleteResponse>
    {
        public string CardToken { get; set; }
    }
}
