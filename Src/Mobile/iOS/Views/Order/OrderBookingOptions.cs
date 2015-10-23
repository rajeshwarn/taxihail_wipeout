using System;
using UIKit;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;
using Cirrious.MvvmCross.Binding.Touch.Views;

namespace apcurium.MK.Booking.Mobile.Client.Views.Order
{
    public partial class OrderBookingOptions : MvxView
	{
		public OrderBookingOptions(IntPtr handle) : base(handle)
		{
		}

		private void Initialize()
		{
            BackgroundColor = UIColor.Clear;

            Subviews[0].BackgroundColor = UIColor.Black.ColorWithAlpha(0.5f);

			FlatButtonStyle.Green.ApplyTo(btnNow);
			FlatButtonStyle.Green.ApplyTo(btnLater);
			FlatButtonStyle.Red.ApplyTo(btnCancel);
		}

		public override void AwakeFromNib()
		{
			base.AwakeFromNib();

			var nib = NibHelper.GetNibForView("OrderBookingOptions");
			var view = (UIView)nib.Instantiate(this, null)[0];
			AddSubview(view);

			Initialize();

			var serviceType = ((OrderOptionsViewModel)DataContext).SelectedVehicleType.ServiceType;
			this.Services().Message.ShowMessage(null, this.Services().Localize["BookATaxi_Message", serviceType == ServiceType.Luxury ? "luxury" : null],

			this.DelayBind (() => {
				lblDescription.Text = Localize.GetValue("BookATaxi_Message");
				btnNow.SetTitle(Localize.GetValue("Now"), UIControlState.Normal);
				btnLater.SetTitle(Localize.GetValue("BookItLaterButton"), UIControlState.Normal);
				btnCancel.SetTitle(Localize.GetValue("Cancel"),UIControlState.Normal);
				InitializeBinding();
			});
		}

		private void InitializeBinding()
		{
			var set = this.CreateBindingSet<OrderBookingOptions, BottomBarViewModel>();

			set.Bind(btnNow)
				.For("TouchUpInside")
				.To(vm => vm.SetPickupDateAndReviewOrder);

			set.Bind(btnLater)
				.For("TouchUpInside")
				.To(vm => vm.BookLater);

			set.Bind(btnCancel)
				.For("TouchUpInside")
				.To(vm => vm.ResetToInitialState);

			set.Apply();
		}
	}
}
