using System;
using Infrastructure.Messaging;
using MK.Common.Configuration;

namespace apcurium.MK.Booking.Commands
{
    public class AddOrUpdateNotificationSettings : ICommand
    {
        public AddOrUpdateNotificationSettings()
        {
            Id = Guid.NewGuid();
        }

        public NotificationSettings NotificationSettings { get; set; }
        public Guid? AccountId { get; set; }
        public Guid CompanyId { get; set; }
        public Guid Id { get; private set; }
    }
}