using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RestService("/account/test/admin/{Index}", "GET")]
    public class TestOnlyReqGetAdminTestAccount : BaseDTO
    {
        public string Index { get; set; }
    }
}
