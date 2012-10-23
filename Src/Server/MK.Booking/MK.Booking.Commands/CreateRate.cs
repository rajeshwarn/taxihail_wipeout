using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class CreateRate : ICommand
    {
        public CreateRate()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }

        public Guid CompanyId { get; set; }

        public Guid RateId { get; set; }

        public double FlatRate { get; set; }
    }
}
