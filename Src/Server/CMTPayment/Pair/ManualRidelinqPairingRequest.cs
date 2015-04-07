using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.ServiceHost;

namespace CMTPayment.Pair
{
    [Route("v1/pairing")]
    public class ManualRideLinqPairingRequest : IReturn<CmtPairingResponse>
    {
        public string PairingCode { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string CallbackUrl { get; set; }
        public int AutoTipPercentage { get; set; }
        public bool AutoCompletePayment { get; set; }
    }
}
