using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;

namespace apcurium.MK.Booking.Api.Client.Cmt.Payments
{
    public class PaymentResponse
    {
        public string ResponseMessage { get; set; }

        public int ResponseCode { get; set; }
    }
}
