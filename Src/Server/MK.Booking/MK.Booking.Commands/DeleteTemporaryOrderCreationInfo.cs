using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class DeleteTemporaryOrderCreationInfo : ICommand
    {
        public DeleteTemporaryOrderCreationInfo()
        {
            Id = Guid.NewGuid();
        }

        public Guid OrderId { get; set; }

        public Guid Id { get; set; }
    }
}