namespace apcurium.MK.Booking.Mobile.Infrastructure
{
    public interface IPushNotificationService
    {
        void RegisterDeviceForPushNotifications (bool force = false);
        void SaveDeviceToken(string deviceToken);
    }
}

