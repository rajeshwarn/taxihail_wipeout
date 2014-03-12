#region

using apcurium.MK.Booking.Api.Client.Cmt.Payments;

#endregion

namespace apcurium.MK.Booking.Api.Client.Payments.CmtPayments.Capture
{
    [Route("v2/merchants/{MerchantToken}/capture")]
    public class CaptureRequest : IReturn<CaptureResponse>
    {
        public long TransactionId { get; set; }

        public LevelThreeData L3Data { get; set; }

        public string MerchantToken { get; set; }
    }
}