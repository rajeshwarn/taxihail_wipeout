using Infrastructure.Messaging;
using System;

namespace apcurium.MK.Booking.Commands
{
    public class UnregisterDeviceForPushNotifications: ICommand
    {
        public UnregisterDeviceForPushNotifications()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; private set; }
        public Guid AccountId { get; set; }
        public string DeviceToken { get; set; }
    }
}
