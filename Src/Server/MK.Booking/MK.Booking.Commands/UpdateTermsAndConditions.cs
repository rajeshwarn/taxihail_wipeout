using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class UpdateTermsAndConditions : ICommand
    {
        public UpdateTermsAndConditions()
        {
            Id = Guid.NewGuid();
        }

        public Guid CompanyId { get; set; }
        public string TermsAndConditions { get; set; }
        public Guid Id { get; private set; }
    }
}