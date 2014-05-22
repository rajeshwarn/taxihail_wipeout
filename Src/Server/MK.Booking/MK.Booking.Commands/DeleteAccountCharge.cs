using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class DeleteAccountCharge : ICommand
    {
        public DeleteAccountCharge()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public Guid AccountChargeId { get; set; }
        public Guid CompanyId { get; set; }
    }
}
