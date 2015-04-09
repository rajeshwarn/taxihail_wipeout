
using System;

using Foundation;
using UIKit;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class ManualPairingForRideLinq : UIViewController
	{
		public ManualPairingForRideLinq()
			: base("ManualPairingForRideLinq", null)
		{
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			FlatButtonStyle.Green.ApplyTo(btnPair);

			var bindingSet = this.CreateBinding()
		}
	}
}

