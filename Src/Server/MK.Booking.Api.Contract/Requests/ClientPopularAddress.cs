using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RestService("/popularaddresses", "GET")]
    [RestService("/admin/popularaddresses", "GET")]
    public class ClientPopularAddress : BaseDTO
    {
        
    }
}
