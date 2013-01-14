using Infrastructure.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Booking.Commands
{
    public class RegisterDeviceForPushNotifications: ICommand
    {
        public RegisterDeviceForPushNotifications()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; private set; }
        public Guid AccountId { get; set; }
        public string DeviceToken { get; set; }
        public string OldDeviceToken { get; set; }
        public PushNotificationServicePlatform Platform { get; set; }
    }
}
