#region

using System;
using apcurium.MK.Common.Entity;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Commands
{
    public class CreateTariff : ICommand
    {
        public CreateTariff()
        {
            Id = Guid.NewGuid();
        }

        public Guid CompanyId { get; set; }

        public Guid TariffId { get; set; }
        public string Name { get; set; }

        public decimal FlatRate { get; set; }
        public double KilometricRate { get; set; }
        public double MarginOfError { get; set; }
        public double KilometerIncluded { get; set; }
        public decimal PassengerRate { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DayOfTheWeek DaysOfTheWeek { get; set; }
        public TariffType Type { get; set; }
        public Guid Id { get; set; }
    }
}