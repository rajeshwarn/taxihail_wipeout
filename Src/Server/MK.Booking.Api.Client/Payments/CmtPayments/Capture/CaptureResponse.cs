using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace apcurium.MK.Booking.Api.Client.Cmt.Payments.Capture
{
    public class CaptureResponse:PaymentResponse
    {

        [XmlElement("transactionId")]
        public long TransactionId { get; set; }

        [XmlElement("authorizationCode")]
        public string AuthorizationCode { get; set; }
        

        [XmlElement("captureDate")]
        public string CaptureDate { get; set; }

        [XmlElement("amount")]
        public int _Amount { get; set; }

        [XmlIgnore]
        public double Amount { get { return ((double)_Amount) / 100; } }

        [XmlElement("currencyCode")]
        public string CurrencyCode { get; set; }

        [XmlElement("cardType")]
        public string CardType { get; set; }

        [XmlElement("employeeId")]
        public string EmployeeId { get; set; }

        [XmlElement("customerReferenceNumber")]
        public string CustomerReferenceNumber { get; set; }

        [XmlElement("truncatedAccountNumber")]
        public string TruncatedAccountNumber { get; set; }

        [XmlElement("expirationDate")]
        public string ExpirationDate { get; set; }
    }
}
