using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.Messaging;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Commands
{
    public class SendReceipt : ICommand
    {
        public SendReceipt()
        {
            Id = Guid.NewGuid();
            CardOnFileInfo = null; // must be null if not used - see email template
        }

        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public string EmailAddress { get; set; }
        public int IBSOrderId { get; set; }
        public DateTime TransactionDate { get; set; }
        public string VehicleNumber { get; set; }
        public double Tip { get; set; }
        public double Fare { get; set; }
        public double Toll { get; set; }
        public double Tax { get; set; }

        public Address PickupAddress { get; set; }
        public Address DropOffAddress { get; set; }
        
        public CardOnFile CardOnFileInfo { get; set; } // OPTIONAL Null if not needed

        public class CardOnFile
        {
            public CardOnFile(decimal amount, string transactionId, string authorizationCode, string company)
            {
                Amount = amount;
                TransactionId = transactionId;
                Company = company;
                AuthorizationCode = authorizationCode;
            }

            public decimal Amount { get; set; }
            public string TransactionId { get; set; }
            public string AuthorizationCode { get; set; }
            public string Company { get; set; }
            public string LastFour { get; set; }
            public string FriendlyName { get; set; }
        }

        public double TotalFare
        {
            get { return Fare + Toll + Tip + Tax; }
        }
    }
}
