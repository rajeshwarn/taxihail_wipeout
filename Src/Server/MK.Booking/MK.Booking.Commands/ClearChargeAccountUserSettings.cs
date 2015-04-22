using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class ClearChargeAccountUserSettings : ICommand
    {
        public ClearChargeAccountUserSettings()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; private set; }

        public Guid[] AccountIds { get; set; }
    }
}
