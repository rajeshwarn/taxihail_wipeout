using Android.App;
using Android.Content.PM;
using Android.OS;
using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
	[Activity(Label = "@string/PromotionActivityName",
        Theme = "@style/MainTheme",
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class PromotionActivity : BaseBindingActivity<PromotionViewModel>
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.View_Promotion);
        }
    }
}