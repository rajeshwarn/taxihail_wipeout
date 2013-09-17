using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace apcurium.MK.Booking.Api.Client.Cmt.Payments.Authorization
{
    public class AuthorizationResponse :PaymentResponse
    {
        public long TransactionId { get; set; }

        public string DeviceName { get; set; }

        public string CustomerReferenceNumber { get; set; }

        public string CardType { get; set; }

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
