using Infrastructure.Messaging;
using System;

namespace apcurium.MK.Booking.Commands
{
    public class SaveOriginalEta : ICommand
    {
        public SaveOriginalEta()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; set; }

        public Guid OrderId { get; set; }

        public long OriginalEta { get; set; }
    }
}
