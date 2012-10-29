using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class DeleteRate : ICommand
    {
        public DeleteRate()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; private set; }
        public Guid CompanyId { get; set; }
        public Guid RateId { get; set; }
    }
}
