using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests.Orders
{
    [Authenticate]
    [Route("/order/payments/{TransactionId}", "POST")]
    public class CapturePaymentRequest : IReturnVoid
    {
        public long TransactionId { get; set; }
        public Guid OrderId { get; set; }

        public string IbsOrderNumber { get; set; }

        public string CarNumber { get; set; }

        public string Amount { get; set; }
    }
}

