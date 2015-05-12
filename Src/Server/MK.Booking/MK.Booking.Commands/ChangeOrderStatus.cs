#region

using System;
using apcurium.MK.Common.Entity;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Commands
{
    public class ChangeOrderStatus : ICommand
    {
        public ChangeOrderStatus()
        {
            Id = Guid.NewGuid();
        }

        public DateTime? DropOffDate { get; set; }
        public OrderStatusDetail Status { get; set; }
        public double? Fare { get; set; }
        public double? Toll { get; set; }
        public double? Tip { get; set; }
        public double? Tax { get; set; }
        public Guid Id { get; private set; }
    }
}