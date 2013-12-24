#region

using System;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Commands
{
    public class DeleteRule : ICommand
    {
        public DeleteRule()
        {
            Id = Guid.NewGuid();
        }

        public Guid CompanyId { get; set; }
        public Guid RuleId { get; set; }
        public Guid Id { get; private set; }
    }
}