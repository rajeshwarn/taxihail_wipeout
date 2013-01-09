using System;

namespace apcurium.MK.Booking.Mobile.Infrastructure
{
    public interface IPushNotificationService
    {
        void RegisterDeviceForPushNotifications ();
        void SaveDeviceToken(string deviceToken);
    }
}

