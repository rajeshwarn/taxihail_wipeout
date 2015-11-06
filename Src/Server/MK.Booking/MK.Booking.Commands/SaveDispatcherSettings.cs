using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class SaveDispatcherSettings : ICommand
    {
        public SaveDispatcherSettings()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; private set; }

        public string Market { get; set; }

        public int NumberOfOffersPerCycle { get; set; }

        public int NumberOfCycles { get; set; }

        public int DurationOfOfferInSeconds { get; set; }
    }
}
