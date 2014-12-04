using System;
using apcurium.MK.Common.Entity;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class ImportAccountCharge : ICommand
    {
        public ImportAccountCharge()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public AccountCharge[] AccountCharges { get; set; }
        public Guid CompanyId { get; set; }
    }
}