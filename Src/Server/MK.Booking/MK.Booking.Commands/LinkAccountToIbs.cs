using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class LinkAccountToIbs : ICommand
    {
        public LinkAccountToIbs()
        {
            Id = Guid.NewGuid();
        }

        public Guid AccountId { get; set; }
        public string CompanyKey { get; set; }
        public int IbsAccountId { get; set; }

        public Guid Id { get; set; }
    }
}