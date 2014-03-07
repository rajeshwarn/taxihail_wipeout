using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class RetriggerTermsAndConditions : ICommand
    {
        public RetriggerTermsAndConditions()
        {
            Id = Guid.NewGuid();
        }

        public Guid CompanyId { get; set; }
        public Guid Id { get; private set; }
    }
}