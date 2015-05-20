using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class UpdateAutoTip : ICommand
    {
        public UpdateAutoTip()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }

        public Guid OrderId { get; set; }

        public int AutoTipPercentage { get; set; }
    }
}
