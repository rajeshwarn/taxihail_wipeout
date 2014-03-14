using System;
using apcurium.MK.Booking.Api.Client.Cmt.Payments;

namespace apcurium.MK.Booking.Api.Client.Payments.CmtPayments.Reverse
{
    public class ReverseResponse : PaymentResponse
    {
        public long TransactionId { get; set; }

        public string DeviceId { get; set; }

        public string CustomerReferenceNumber { get; set; }

        public string CardType { get; set; }

        public DateTime AuthorizationDate { get; set; }

        public string AuthorizationCode { get; set; }

        public int Amount { get; set; }

        public string CurrencyCode { get; set; }

        public string EmployeeId { get; set; }

        public int AuthTimeMillis { get; set; }

        public string CardOnFileToken { get; set; }

        public string TruncatedAccountNumber { get; set; }

        public string ExpirationDate { get; set; }
    }
}