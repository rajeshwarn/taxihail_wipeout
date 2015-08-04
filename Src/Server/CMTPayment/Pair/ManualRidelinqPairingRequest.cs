using ServiceStack.ServiceHost;

namespace CMTPayment.Pair
{
    [Route("init/pairing/{PairingToken}")]
    public class ManualRideLinqPairingRequest : IReturn<CmtUnpairingResponse>
    {
        public string CustomerId { get; set; }

        public string CustomerName { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public bool AutoCompletePayment { get; set; }

        public int AutoTipPercentage { get; set; }
    }
}
