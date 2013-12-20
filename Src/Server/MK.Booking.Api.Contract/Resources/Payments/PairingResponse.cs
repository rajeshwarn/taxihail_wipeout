namespace apcurium.MK.Booking.Api.Contract.Resources.Payments
{
    public class PairingResponse : BasePaymentResponse
    {
        public string PairingToken { get; set; }
        public string PairingCode { get; set; }
        public string Medallion { get; set; }
        public int TripId { get; set; }
        public int DriverId { get; set; }
    }
}