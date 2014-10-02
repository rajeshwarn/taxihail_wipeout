using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class MigrateAppSettingNames : ICommand
    {
        public MigrateAppSettingNames()
        {
            Id = Guid.NewGuid();
        }

        public Guid CompanyId { get; set; }
        public Guid Id { get; set; }
    }
}
