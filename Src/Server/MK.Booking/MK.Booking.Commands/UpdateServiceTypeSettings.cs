using System;
using Infrastructure.Messaging;
using MK.Common.Configuration;

namespace apcurium.MK.Booking.Commands
{
    public class UpdateServiceTypeSettings : ICommand
    {
        public UpdateServiceTypeSettings()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }

        public Guid CompanyId { get; set; }

        public ServiceTypeSettings ServiceTypeSettings { get; set; }
    }
}