using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace TaxiMobile.Activities.Setting
{
	[Activity (Label = "SettingsGroup", ScreenOrientation=ScreenOrientation.Portrait)]			
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

