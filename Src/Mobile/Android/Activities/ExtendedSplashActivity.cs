using apcurium.MK.Booking.Mobile.ViewModels;
using Android.App;
using Android.Content.PM;

namespace apcurium.MK.Booking.Mobile.Client.Activities
{
	[Activity(
		Label = "@string/ApplicationName", 
		Theme = "@style/Theme.Splash",
		NoHistory = true,
		Icon = "@drawable/icon",
		ScreenOrientation = ScreenOrientation.Portrait)]
	public class ExtendedSplashActivity : BaseBindingActivity<ExtendedSplashScreenViewModel>
	{

	}
}