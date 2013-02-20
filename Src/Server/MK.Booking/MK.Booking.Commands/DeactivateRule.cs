using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class DeactivateRule : ICommand
    {
        public DeactivateRule()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; private set; }
        public Guid CompanyId { get; set; }
        public Guid RuleId { get; set; }
    }
}
