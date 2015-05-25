using System;
using System.Collections.Generic;
using apcurium.MK.Common.Entity;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class UpdateFees : ICommand
    {
        public UpdateFees()
        {
            Id = Guid.NewGuid();
        }

        public Guid CompanyId { get; set; }
        public List<Fees> Fees { get; set; }
        public Guid Id { get; private set; }
    }
}