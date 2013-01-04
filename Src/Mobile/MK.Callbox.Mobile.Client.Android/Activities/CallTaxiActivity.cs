using Android.App;
using Android.OS;
using Cirrious.MvvmCross.Binding.Android.Views;
using apcurium.MK.Booking.Mobile.ViewModels.Callbox;

namespace apcurium.MK.Callbox.Mobile.Client.Activities
{
    [Activity(Label = "Call Taxi", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
    public class CallTaxiActivity : MvxBindingActivityView<CallboxCallTaxiViewModel>
    {
        protected override void OnViewModelSet()
        {
            SetContentView(Resource.Layout.View_CallTaxi);
        }
    }
}