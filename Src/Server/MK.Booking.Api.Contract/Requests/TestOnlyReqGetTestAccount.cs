using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RestService("/accounts/test/{Index}", "GET")]
    public class TestOnlyReqGetTestAccount : BaseDTO
    {
        public string Index { get; set; }
    }
}
