using System;
using System.ComponentModel.DataAnnotations;

namespace apcurium.MK.Booking.ReadModel
{
    public class DispatcherSettingsDetail
    {
        public DispatcherSettingsDetail()
        {
            Id = Guid.NewGuid();
        }

        [Key]
        public Guid Id { get; set; }

        public string Market { get; set; }

        public int NumberOfOffersPerCycle { get; set; }

        public int NumberOfCycles { get; set; }

        public int DurationOfOfferInSeconds { get; set; }
    }
}
