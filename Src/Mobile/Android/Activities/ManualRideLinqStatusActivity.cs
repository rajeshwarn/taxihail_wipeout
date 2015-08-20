using apcurium.MK.Booking.Mobile.ViewModels;
using Android.App;
using Android.Content.PM;
using Android.Widget;

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

			var lblMedallion = FindViewById<TextView>(Resource.Id.lblManualRideLinqStatus_Medallion);
			var lblEmail = FindViewById<TextView>(Resource.Id.lblManualRideLinqStatus_Email);

			lblMedallion.Text = string.Format(this.Services ().Localize["ManualRideLinqStatus_Medallion"], ViewModel.Medallion);
			lblEmail.Text = string.Format(this.Services ().Localize["ManualRideLinqStatus_Email"], ViewModel.Email);
        }
    }
}