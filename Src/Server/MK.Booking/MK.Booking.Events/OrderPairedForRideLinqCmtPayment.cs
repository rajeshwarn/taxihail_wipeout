using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class OrderPairedForRideLinqCmtPayment : VersionedEvent
    {
        public string Medallion { get; set; }
        public string DriverId { get; set; }
        public string PairingToken { get; set; }
        public string PairingCode { get; set; }
        public string TokenOfCardToBeUsedForPayment { get; set; }
        public double? AutoTipAmount { get; set; }
        public int? AutoTipPercentage { get; set; }
    }
}