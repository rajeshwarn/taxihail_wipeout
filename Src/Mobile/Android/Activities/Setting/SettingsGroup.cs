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

namespace apcurium.MK.Booking.Mobile.Client.Activities.Setting
{
	[Activity (Label = "SettingsGroup", ScreenOrientation=Android.Content.PM.ScreenOrientation.Portrait)]			
	public class SettingsGroup : TabGroupActivity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			var intent = new Intent().SetClass(this, typeof(SettingsActivity));
			StartChildActivity("Settings", intent );
		}
	}
}

