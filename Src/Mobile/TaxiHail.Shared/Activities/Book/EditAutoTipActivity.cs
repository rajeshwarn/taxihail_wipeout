using apcurium.MK.Booking.Mobile.ViewModels;
using Android.App;
using Android.Content.PM;
using Android.OS;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
    [Activity(Label = "EditAutoTipActivity", Theme = "@style/MainTheme", ScreenOrientation = ScreenOrientation.Portrait)]
    public class EditAutoTipActivity : BaseBindingActivity<EditAutoTipViewModel>
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.View_EditAutoTip);
        }
    }
}