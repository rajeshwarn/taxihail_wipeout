using System;
using UIKit;
using ObjCRuntime;


namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public partial class ManualPairingForRideLinqViewModel
	{
		partial void HideIOSKeyboard()
		{
			// hide the keyboard when command is pressed
			UIApplication.SharedApplication.SendAction (Selector.FromHandle(Selector.GetHandle("resignFirstResponder")), null, null, null);
		}
	}
}

