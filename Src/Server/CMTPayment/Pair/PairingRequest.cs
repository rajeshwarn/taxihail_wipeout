using ServiceStack.ServiceHost;

namespace CMTPayment.Pair
{
    [Route("init/pairing/external/cof")]
    public class PairingRequest : IReturn<CmtPairingResponse>
    {
        public string PairingCode { get; set; }

        public string Medallion { get; set; }

        public string DriverId { get; set; }

        public string CustomerId { get; set; }

        public string CustomerName { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public string CallbackUrl { get; set; }

        public bool AutoCompletePayment { get; set; }

        public int? AutoTipPercentage { get; set; }

        public string CardOnFileId { get; set; }
        
        public string Market { get; set; }

        public string TripRequestNumber { get; set; }

        public string LastFour { get; set; }
        
        public double? TipIncentive { get; set; }
    }
}