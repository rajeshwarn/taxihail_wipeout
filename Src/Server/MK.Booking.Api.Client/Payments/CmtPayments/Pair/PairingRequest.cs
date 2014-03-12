#region

using ServiceStack.ServiceHost;

#endregion



namespace apcurium.MK.Booking.Api.Client.Payments.CmtPayments.Pair
{
    [Route("v1/init/pairing/external/cof")]
    public class PairingRequest : IReturn<CmtPairingResponse>
    {
        public string Medallion { get; set; }
        public string DriverId { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string CallbackUrl { get; set; }
        public bool AutoCompletePayment { get; set; }
        public int? AutoTipPercentage { get; set; }
        public double? AutoTipAmount { get; set; }
        public string CardOnFileId { get; set; }
    }
}