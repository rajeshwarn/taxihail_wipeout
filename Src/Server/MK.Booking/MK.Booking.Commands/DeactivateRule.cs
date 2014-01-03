#region

using System;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Commands
{
    public class DeactivateRule : ICommand
    {
        public DeactivateRule()
        {
            Id = Guid.NewGuid();
        }

        public Guid CompanyId { get; set; }
        public Guid RuleId { get; set; }
        public Guid Id { get; private set; }
    }
}