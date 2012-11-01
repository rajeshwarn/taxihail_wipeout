using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.Messaging;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Commands
{
    public class UpdateTariff : ICommand
    {
        public UpdateTariff()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; private set; }
        public Guid CompanyId { get; set; }
        public Guid TariffId { get; set; }
        public string Name { get; set; }
        public decimal FlatRate { get; set; }
        public double KilometricRate { get; set; }
        public double MarginOfError { get; set; }
        public decimal PassengerRate { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DayOfTheWeek DaysOfTheWeek { get; set; }
    }
}
