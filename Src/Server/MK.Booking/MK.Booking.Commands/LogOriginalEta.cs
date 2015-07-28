using Infrastructure.Messaging;
using System;

namespace apcurium.MK.Booking.Commands
{
    public class LogOriginalEta : ICommand
    {
        public LogOriginalEta()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; set; }

        public Guid OrderId { get; set; }

        public long OriginalEta { get; set; }
    }
}
