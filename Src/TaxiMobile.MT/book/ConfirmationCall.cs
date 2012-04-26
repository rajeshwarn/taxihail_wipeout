using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using TaxiMobile.Localization;

namespace TaxiMobile.Book
{
	public class Confirmation
	{
		public Confirmation ()
		{
		}



		public void Call (string phone, string phoneDisplay)
		{			
			
			var confirmAlert = new UIAlertView ( phoneDisplay, "", null, Resources.CancelBoutton, null);
			confirmAlert.AddButton (Resources.CallButton);
			confirmAlert.Clicked += delegate(object sender, UIButtonEventArgs e) {
				if (e.ButtonIndex == 1) {
					NSUrl url = new NSUrl ("tel://" + phone);
					
					if (!UIApplication.SharedApplication.OpenUrl (url)) {
						
						
						var av = new UIAlertView ("Not supported", "Calls are not supported on this device", null, Resources.Close, null);
						av.Show ();
					}
				}
				
			};
			
			confirmAlert.Show();					
		}
		
		
		public void Action (string message, Action action )
		{			
			
			var confirmAlert = new UIAlertView ( message, "", null, Resources.NoButton, null);
			confirmAlert.AddButton (Resources.YesButton);
			confirmAlert.Clicked += delegate(object sender, UIButtonEventArgs e) {
				if (e.ButtonIndex == 1) {
					action();
				}
				
			};
			
			confirmAlert.Show();					
		}
		
	}
}

