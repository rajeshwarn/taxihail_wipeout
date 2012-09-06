using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class CompleteOrder : ICommand
    {
        public CompleteOrder()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public DateTime Date { get; set; }
        public double? Fare { get; set; }
        public double? Toll { get; set; }
        public double? Tip { get; set; }
    }
}