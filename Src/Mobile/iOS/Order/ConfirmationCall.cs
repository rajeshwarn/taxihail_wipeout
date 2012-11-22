using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using MonoTouch.CoreFoundation;
namespace apcurium.MK.Booking.Mobile.Client
{
	public class Confirmation
	{
		public Confirmation ()
		{
		}

		public void Call (string phone, string phoneDisplay)
		{			
            throw new InvalidOperationException("Deprecated, see IPhoneService.Call");
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

