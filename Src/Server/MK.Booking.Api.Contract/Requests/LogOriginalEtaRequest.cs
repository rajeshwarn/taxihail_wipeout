using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [Route("/order/logeta", "POST")]
    public class LogOriginalEtaRequest
    {
        public Guid OrderId { get; set; }
        public long? OriginalEta { get; set; }
    }
}
