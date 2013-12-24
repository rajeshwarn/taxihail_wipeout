#region

using System;
using apcurium.MK.Common.Enumeration;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Commands
{
    public class RegisterDeviceForPushNotifications : ICommand
    {
        public RegisterDeviceForPushNotifications()
        {
            Id = Guid.NewGuid();
        }

        public Guid AccountId { get; set; }
        public string DeviceToken { get; set; }
        public string OldDeviceToken { get; set; }
        public PushNotificationServicePlatform Platform { get; set; }
        public Guid Id { get; private set; }
    }
}