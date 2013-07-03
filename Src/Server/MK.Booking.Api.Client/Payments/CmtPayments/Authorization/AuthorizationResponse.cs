using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace apcurium.MK.Booking.Api.Client.Cmt.Payments.Authorization
{
    public class AuthorizationResponse :PaymentResponse
    {
        [XmlElement("transactionId")]
        public long TransactionId { get; set; }

        [XmlElement("deviceName")]
        public string DeviceName { get; set; }

        [XmlElement("customerReferenceNumber")]
        public string CustomerReferenceNumber { get; set; }

        [XmlElement("cardType")]
        public string CardType { get; set; }

        [XmlElement("authorizationDate")]
        public string AuthorizationCode { get; set; }
        
        [XmlElement("amount")]
        public int _Amount { get; set; }

        [XmlIgnore]
        public double Amount { get { return ((double)_Amount) / 100; } }


        [XmlElement("currencyCode")]
        public string CurrencyCode { get; set; }

        [XmlElement("employeeId")]
        public string EmployeeId { get; set; }

        [XmlElement("authTimeMillis")]
        public int AuthTimeMillis { get; set; }

        [XmlElement("cardOnFileToken")]
        public string CardOnFileToken { get; set; }

        [XmlElement("truncatedAccountNumber")]
        public string TruncatedAccountNumber { get; set; }

        [XmlElement("expirationDate")]
        public string ExpirationDate { get; set; }
    }
}
