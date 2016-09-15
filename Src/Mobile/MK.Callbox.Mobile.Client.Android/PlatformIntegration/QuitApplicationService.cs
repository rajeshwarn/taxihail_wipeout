using apcurium.MK.Booking.Mobile.Infrastructure;
using Android.Content;
using Android.OS;
using Cirrious.CrossCore.Droid.Platform;

namespace apcurium.MK.Callbox.Mobile.Client.PlatformIntegration
{
    public class QuitApplicationService : IQuitApplicationService
    {
        readonly IMvxAndroidCurrentTopActivity _topActivity;

        public QuitApplicationService(IMvxAndroidCurrentTopActivity topActivity)
        {
            _topActivity = topActivity;
        }

        public void Quit()
        {
            var pid = Process.MyPid();
            var intent = new Intent(Intent.ActionMain);
            intent.AddCategory(Intent.CategoryHome);
            _topActivity.Activity.StartActivity(intent);
            Process.KillProcess(pid);
        }
    }
}