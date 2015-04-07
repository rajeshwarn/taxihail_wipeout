using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.ServiceHost;

namespace CMTPayment.Pair
{
    [Route("v1/pairing/{PairingToken}")]
    public class ManualRideLinqUnpairingRequest : IReturn<CmtUnpairingResponse>
    {
        public string PairingToken { get; set; }
    }
}
