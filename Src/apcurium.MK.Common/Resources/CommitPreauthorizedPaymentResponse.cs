using System;

namespace apcurium.MK.Common.Resources
{
    public class CommitPreauthorizedPaymentResponse : BasePaymentResponse
    {
        // completed transaction code
        public string AuthorizationCode { get; set; }

        /// <summary>
        /// Represent the id used in the external payment provider (braintree, moneris, etc.) 
        /// Could be null if something failed before we could get to the payment provider
        /// !! Only used for reporting purpose for now
        /// </summary>
        public string TransactionId { get; set; }

        public DateTime? TransactionDate { get; set; }

        public bool IsDeclined { get; set; }
    }
}