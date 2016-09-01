using System;

namespace apcurium.MK.Common.Resources
{
    public class PreAuthorizePaymentResponse : BasePaymentResponse
    {
        public string TransactionId { get; set; }

        public DateTime? TransactionDate { get; set; }

        public string ReAuthOrderId { get; set; }

        public bool IsDeclined { get; set; }
    }
}