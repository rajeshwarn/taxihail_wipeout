using Android.App;
using Android.OS;
using Cirrious.MvvmCross.Droid.Views;
using Android.Views;
using Android.Content.PM;

namespace apcurium.MK.Booking.Mobile.Client.Activities
{
    [Activity(Label = "Debug", 
        WindowSoftInputMode = SoftInput.AdjustResize, 
        ScreenOrientation = ScreenOrientation.Portrait
    )]
    public class DebugActivity : MvxActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            RequestWindowFeature(WindowFeatures.NoTitle);

            SetContentView(Resource.Layout.View_Debug);
        }
    }
}