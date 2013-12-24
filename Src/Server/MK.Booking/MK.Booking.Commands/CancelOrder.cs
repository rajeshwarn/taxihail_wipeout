#region

using System;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Commands
{
    public class CancelOrder : ICommand
    {
        public CancelOrder()
        {
            Id = Guid.NewGuid();
        }

        public Guid OrderId { get; set; }
        public Guid Id { get; set; }
    }
}