#region

using System;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Commands
{
    public class RemoveOrderFromHistory : ICommand
    {
        public RemoveOrderFromHistory()
        {
            Id = Guid.NewGuid();
        }

        public Guid OrderId { get; set; }
        public Guid Id { get; private set; }
    }
}