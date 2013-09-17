using ServiceStack.ServiceHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace apcurium.MK.Booking.Api.Client.Cmt.Payments.Capture
{
    [Route("v2/merchants/{MerchantToken}/capture")]
    public class CaptureRequest : IReturn<CaptureResponse>
    {
        
        public long TransactionId { get; set; }

        public LevelThreeData L3Data { get; set; }

        public string MerchantToken { get; set; }
    }
}
