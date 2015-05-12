﻿using ServiceStack.ServiceHost;

namespace CMTPayment.Pair
{
    [Route("v1/init/pairing/external/cof")]
    public class ManualRideLinqPairingRequest : IReturn<CmtPairingResponse>
    {
        public string PairingCode { get; set; }

        public string CustomerId { get; set; }

        public string CustomerName { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public int AutoTipPercentage { get; set; }

        public bool AutoCompletePayment { get; set; }

        public string CardOnFileId { get; set; }
    }
}
