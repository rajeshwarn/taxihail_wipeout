namespace apcurium.MK.Booking.Api.Client.Cmt.Payments.Pair
{
    public class CmtPairingResponse
    {
        public string PairingToken { get; set; }
        public string PairingCode { get; set; }
        public string Medallion { get; set; }
        public int TripId { get; set; }
        public int DriverId { get; set; }
        public long TimeoutSeconds { get; set; }
    }
}