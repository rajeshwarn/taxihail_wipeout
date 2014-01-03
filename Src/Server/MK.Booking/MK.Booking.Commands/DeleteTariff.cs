#region

using System;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Commands
{
    public class DeleteTariff : ICommand
    {
        public DeleteTariff()
        {
            Id = Guid.NewGuid();
        }

        public Guid CompanyId { get; set; }
        public Guid TariffId { get; set; }
        public Guid Id { get; private set; }
    }
}