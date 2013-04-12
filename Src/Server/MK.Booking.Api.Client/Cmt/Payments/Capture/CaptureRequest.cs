using ServiceStack.ServiceHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace apcurium.MK.Booking.Api.Client.Cmt.Payments.Capture
{
    [Route("capture")]
    public class CaptureRequest : IReturn<CaptureResponse>
    {
        
        [XmlElement("transactionId")]
        public long TransactionId { get; set; }


    }
}
