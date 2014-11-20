using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class UpdatePrivacyPolicy : ICommand
    {
        public UpdatePrivacyPolicy()
        {
            Id = Guid.NewGuid();
        }

        public Guid CompanyId { get; set; }
        public string Policy { get; set; }
        public Guid Id { get; private set; }
    }
}