using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class DeleteTariff : ICommand
    {
        public DeleteTariff()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; private set; }
        public Guid CompanyId { get; set; }
        public Guid TariffId { get; set; }
    }
}
