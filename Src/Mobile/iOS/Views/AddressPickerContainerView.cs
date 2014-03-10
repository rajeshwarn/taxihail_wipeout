using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using apcurium.MK.Booking.Mobile.Client.Views.AddressPicker;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class AddressPickerContainerView : BaseViewController<AddressPickerViewModel>
	{
		public AddressPickerContainerView () : base ("AddressPickerContainerView", null)
		{
		}

		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			NavigationController.NavigationBar.Hidden = true;
			View.BackgroundColor = UIColor.FromRGB(242, 242, 242);

			var addressPickerView = View.Subviews [0] as AddressPickerView;
			addressPickerView.Hidden = false;
			addressPickerView.DataContext = ViewModel;
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
		
			// Perform any additional setup after loading the view, typically from a nib.
		}
	}
}

