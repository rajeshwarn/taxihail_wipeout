namespace CMTPayment.Tokenize
{
    public class BasePaymentValidationRequest
    {
        public string ZipCode { get; set; }

        /// <summary>
        /// Fraud session id (Kount)
        /// </summary>
        /// <value>The session identifier.</value>
        public string SessionId { get; set; }

        /// <summary>
        /// Note: This field is only used if the first name and last name are not provided
        /// </summary>
        public string BillingFullName { get; set; }

        /// <summary>
        /// Warning: Don't send it now since we only have fullname
        /// </summary>
        /// <value>The first name for the billing info.</value>
        public string BillingFirstName { get; set; }

        /// <summary>
        /// Warning: Don't send it now since we only have fullname
        /// </summary>
        /// <value>The last name for the billing info.</value>
        public string BillingLastName { get; set; }

        /// <summary>
        /// Warning: We don't have this information
        /// </summary>
        /// <value>The billing address.</value>
        public string BillingAddress { get; set; }

        /// <summary>
        /// Warning: We don't have this information
        /// </summary>
        /// <value>The billing city.</value>
        public string BillingCity { get; set; }

        /// <summary>
        /// Warning: We don't have this information
        /// </summary>
        /// <value>The state of the billing.</value>
        public string BillingState { get; set; }

        /// <summary>
        /// Warning: We don't have this information (phone number is possibly not billing phone)
        /// </summary>
        /// <value>The billing phone.</value>
        public string BillingPhone { get; set; }

        public string Email { get; set; }

        public string CustomerIpAddress { get; set; }
    }
}