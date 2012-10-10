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

using TinyIoC;
using apcurium.MK.Booking.Mobile.Localization;
using apcurium.MK.Booking.Mobile.Infrastructure;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Setting
{
	[Activity (Label = "About", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation=Android.Content.PM.ScreenOrientation.Portrait)]
    public class AboutActivity : BaseActivity
	{
        protected override int ViewTitleResourceId
        {
            get { return Resource.String.View_About;  }
        }

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			SetContentView(Resource.Layout.View_About);

            string aboutUrl = TinyIoCContainer.Current.Resolve<IAppSettings>().SiteUrl; 			
			FindViewById<WebView>(Resource.Id.aboutWebView).LoadUrl( aboutUrl ); 
            FindViewById<WebView>(Resource.Id.aboutWebView).SetInitialScale(40);
		}


	}
}

