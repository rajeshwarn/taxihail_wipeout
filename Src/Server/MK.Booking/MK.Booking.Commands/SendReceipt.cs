using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class SendReceipt : ICommand
    {
        public SendReceipt()
        {
            Id = Guid.NewGuid();
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

        public double TotalFare
        {
            get { return Fare + Toll + Tip + Tax; }
        }
    }
}
