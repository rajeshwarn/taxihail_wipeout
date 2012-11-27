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
using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Setting
{
	[Activity (Label = "About", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation=Android.Content.PM.ScreenOrientation.Portrait)]
    public class AboutActivity : BaseBindingActivity<AboutUsViewModel>
	{
        protected override int ViewTitleResourceId
        {
            get { return Resource.String.View_About;  }
        }

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			SetContentView(Resource.Layout.View_About);
			FindViewById<WebView>(Resource.Id.aboutWebView).LoadUrl( ViewModel.Uri );
            FindViewById<WebView>(Resource.Id.aboutWebView).SetInitialScale(40);

		}

	    protected override void OnViewModelSet()
	    {
	    }
	}
}

