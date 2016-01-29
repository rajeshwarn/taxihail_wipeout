namespace apcurium.MK.Common.Resources
{
    public class PairingResponse : BasePaymentResponse
    {
        public string PairingToken { get; set; }
        public string PairingCode { get; set; }
        public string Medallion { get; set; }
        public int TripId { get; set; }
        public int DriverId { get; set; }
        public int? ErrorCode { get; set; }
    }
}