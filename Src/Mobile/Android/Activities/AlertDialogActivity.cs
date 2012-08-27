
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
using apcurium.MK.Booking.Mobile.Client.Helpers;

namespace apcurium.MK.Booking.Mobile.Client
{
	[Activity (Label = "AlertDialogActivity", Theme ="@android:style/Theme.Dialog")]			
	public class AlertDialogActivity : Activity
	{
		private string _title;
		private string _message;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			_title = Intent.GetStringExtra("Title");
			_message = Intent.GetStringExtra("Message");
		}

		protected override void OnStart ()
		{
			base.OnStart ();
			DisplayError();
		}

		private void DisplayError( )
		{
			AlertDialogHelper.ShowAlert( this, _title, _message, () => { 
				Console.WriteLine("Error Raised");
				Finish();
			} );
		}
	}
}

