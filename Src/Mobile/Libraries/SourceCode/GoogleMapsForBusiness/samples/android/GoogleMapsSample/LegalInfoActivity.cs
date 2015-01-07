using System;
using Android.App;
using Android.Widget;
using Android.Gms.Common;

namespace GoogleMapsSample
{
	[Activity (Label = "@string/legal_info")]
	public class LegalInfoActivity : Activity
	{
		protected override void OnCreate (Android.OS.Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			SetContentView (Resource.Layout.legal_info);

			var legalInfoTextView = FindViewById<TextView> (Resource.Id.legal_info);
			string openSourceSoftwareLicenseInfo =
				GooglePlayServicesUtil.GetOpenSourceSoftwareLicenseInfo (this);
			if (openSourceSoftwareLicenseInfo != null) {
				legalInfoTextView.Text = openSourceSoftwareLicenseInfo;
			} else {
				legalInfoTextView.SetText (Resource.String.play_services_not_installed);
			}
		}
	}
}

