using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class DeleteRule : ICommand
    {
        public DeleteRule()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; private set; }
        public Guid CompanyId { get; set; }
        public Guid RuleId { get; set; }
    }
}
