using System;
using apcurium.MK.Common.Entity;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class AddUpdateAccountCharge : ICommand
    {
        public AddUpdateAccountCharge()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public Guid AccountChargeId { get; set; }
        public string Name { get; set; }
        public string Number { get; set; }
        public AccountChargeQuestion[] Questions { get; set; }
        public Guid CompanyId { get; set; }
    }
}