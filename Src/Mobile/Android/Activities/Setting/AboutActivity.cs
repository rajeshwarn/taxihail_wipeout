using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Webkit;

using Microsoft.Practices.ServiceLocation;
using TaxiMobileApp;

namespace TaxiMobile.Activities.Setting
{
	[Activity (Label = "About", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation=Android.Content.PM.ScreenOrientation.Portrait)]			
	public class AboutActivity : Activity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			SetContentView(Resource.Layout.About);
			
			string aboutUrl = "";
			if( ServiceLocator.Current.GetInstance<IAppResource>().CurrentLanguage == TaxiMobileApp.AppLanguage.Francais )
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

