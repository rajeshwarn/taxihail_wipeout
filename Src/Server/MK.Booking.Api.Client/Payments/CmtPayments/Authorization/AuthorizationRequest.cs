using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Client.Cmt.Payments.Authorization
{
    [Route("/v2/authorize")]
    public class AuthorizationRequest : IReturn<AuthorizationResponse>
    {
        public AuthorizationRequest()
        {
            CardReaderMethod = CardReaderMethods.Manual;
        }

        [XmlElement("transactionType")]
        public string TransactionType { get; set; }

        [XmlElement("amount")]
        public int Amount { get; set; }

        [XmlElement("cardReaderMethod")]
        public int CardReaderMethod { get; set; }

        [XmlElement("cardOnFileToken")]
        public string CardOnFileToken { get; set; }

        [XmlElement("currencyCode")]
        public string CurrencyCode { get; set; }

        [XmlElement("L3Data")]
        public LevelThreeData L3Data { get; set; }

        public class TransactionTypes
        {
            public const string Sale = "S";
            public const string PreAuthorized = "P";
        }

        public class CardReaderMethods
        {
            public const int Swipe = 0;
            public const int RfidTap = 1;
            public const int Manual = 2;
        }


    }
}
