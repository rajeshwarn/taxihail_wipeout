#region

using System;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Commands
{
    public class UnregisterDeviceForPushNotifications : ICommand
    {
        public UnregisterDeviceForPushNotifications()
        {
            Id = Guid.NewGuid();
        }

        public Guid AccountId { get; set; }
        public string DeviceToken { get; set; }
        public Guid Id { get; private set; }
    }
}