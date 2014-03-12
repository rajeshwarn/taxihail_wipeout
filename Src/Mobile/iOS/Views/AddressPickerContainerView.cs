using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using apcurium.MK.Booking.Mobile.Client.Views.AddressPicker;
using Cirrious.MvvmCross.Binding.BindingContext;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
    public partial class AddressPickerContainerView : BaseViewController<AddressPickerViewModel>
	{
		public AddressPickerContainerView () : base ("AddressPickerContainerView", null)
		{
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			NavigationController.NavigationBar.BarStyle = UIBarStyle.Default;
			NavigationController.NavigationBar.Hidden = true;
			View.BackgroundColor = UIColor.FromRGB(242, 242, 242);
			View.Hidden = false;
            View.Bind(this, "DataContext;");
			((AddressPickerView)View).FocusOnTextField ();
		}
	}
}

