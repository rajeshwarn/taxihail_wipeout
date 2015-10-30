
using System;

using Foundation;
using UIKit;
using apcurium.MK.Booking.Mobile.Client.Controls.Binding;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using apcurium.MK.Booking.Mobile.Client.Localization;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Binding.Touch.Views;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class BookingBarManualRideLinQ: MvxView
	{
		public static BookingBarManualRideLinQ LoadViewFromFile()
		{
			var bookingView = NSBundle.MainBundle.LoadNib("BookingBarManualRideLinQ", null, null).GetItem<BookingBarManualRideLinQ>(0);

			bookingView.buttonEstimate.Initialize(Localize.GetValue("Destination"), "destination_small_icon.png", "destination_small_icon_pressed.png");

			bookingView.buttonManualRideLinQBooking.SetTitle(Localize.GetValue("HomeView_ManualPairing"), UIControlState.Normal);
			FlatButtonStyle.Blue.ApplyTo(bookingView.buttonManualRideLinQBooking);

			FlatButtonStyle.Green.ApplyTo(bookingView.buttonBooking);

			return bookingView;
		}

		public BookingBarManualRideLinQ(IntPtr handle):base(handle)
		{
			this.DelayBind(DataBinding);
		}

		public void DataBinding()
		{
			var set = this.CreateBindingSet<BookingBarManualRideLinQ, BottomBarViewModel>();

			set.Bind(this).For(v => v.Hidden).To(vm => vm.HideManualRideLinqButtons);

			set.Bind(buttonEstimate).For(v => v.Command).To(vm => vm.ChangeAddressSelectionMode);
			set.Bind(buttonEstimate).For(v => v.Selected).To(vm => vm.EstimateSelected);
			set.Bind(viewEstimate).For(v => v.Hidden).To(vm => vm.Settings.HideDestination);

			set.Bind(viewBooking).For(v => v.Hidden).To(vm => vm.BookButtonHidden);
			set.Bind(buttonBooking).For(v => v.Command).To(vm => vm.Book);
			set.Bind(buttonBooking).For(v => v.Enabled).To(vm => vm.ParentViewModel.Map.BookCannotExecute).WithConversion("BoolInverter");
			set.Bind(buttonBooking).For("Title").To(vm => vm.BookButtonText);

			set.Bind(buttonManualRideLinQBooking).For(v => v.Command).To(vm => vm.ManualPairingRideLinq);

			set.Bind(imagePromotion).For(v => v.Hidden).To(vm => vm.IsPromoCodeActive).WithConversion("BoolInverter");

			set.Apply();
		}
	}
}