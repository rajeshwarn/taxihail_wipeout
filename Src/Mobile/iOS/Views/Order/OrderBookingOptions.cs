using System;
using UIKit;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using apcurium.MK.Booking.Mobile.Client.Controls.Binding;
using TinyIoC;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;

namespace apcurium.MK.Booking.Mobile.Client.Views.Order
{
	public partial class OrderBookingOptions : BaseBindableChildView<BottomBarViewModel>
	{
		public OrderBookingOptions(IntPtr handle) : base(handle)
		{
		}

		private void Initialize()
		{
			BackgroundColor = UIColor.FromRGBA(68, 68, 68, 50);

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

			this.DelayBind (() => {
				InitializeBinding();
			});
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			//constraintHeight.Constant = this.Frame.Height;
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

			set.Apply();
		}
	}
}
