using apcurium.MK.Booking.Mobile.ViewModels;
using Android.App;
using Android.Content.PM;

namespace apcurium.MK.Booking.Mobile.Client.Activities
{
    [Activity(Theme = "@style/MainTheme",
		   Label = "@string/ManualRideLinqStatusActivityName",
           ScreenOrientation = ScreenOrientation.Portrait
         )]  
    public class ManualRideLinqStatusActivity : BaseBindingActivity<ManualRideLinqStatusViewModel>
    {
        protected override void OnViewModelSet()
        {
            base.OnViewModelSet();

            SetContentView(Resource.Layout.View_ManualRideLinqStatus);
        }
    }
}