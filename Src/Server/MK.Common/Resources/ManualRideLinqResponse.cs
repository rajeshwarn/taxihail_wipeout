using System;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Common.Resources
{
    public class ManualRideLinqResponse : BasePaymentResponse
    {
        public OrderManualRideLinqDetail Data { get; set; }

		public string ErrorCode { get; set; }

		public int TripInfoHttpStatusCode { get; set; }
    }
}