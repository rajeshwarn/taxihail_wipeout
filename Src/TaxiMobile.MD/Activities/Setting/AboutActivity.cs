using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Webkit;
using Microsoft.Practices.ServiceLocation;
using TaxiMobileApp;

namespace TaxiMobile.Activities.Setting
{
	[Activity (Label = "About", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation=ScreenOrientation.Portrait)]			
	public class AboutActivity : Activity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			SetContentView(Resource.Layout.About);
			
			string aboutUrl = "";
			if( ServiceLocator.Current.GetInstance<IAppResource>().CurrentLanguage == AppLanguage.Francais )
			{
				aboutUrl = @"file:///android_asset/About_fr.html";
			}
			else
			{
				aboutUrl = @"file:///android_asset/About_en.html";
			}
			
			FindViewById<WebView>(Resource.Id.aboutWebView).LoadUrl( aboutUrl );
		}
	}
}

