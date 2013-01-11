
using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Booking.PushNotifications
{
    public interface IPushNotificationService
    {
        void Send(string alert, string deviceToken, PushNotificationServicePlatform platform);
    }
}
