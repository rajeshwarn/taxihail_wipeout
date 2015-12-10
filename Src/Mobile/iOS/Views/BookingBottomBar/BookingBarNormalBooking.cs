
using System;
using Foundation;
using apcurium.MK.Booking.Mobile.Client.Localization;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using Cirrious.MvvmCross.Binding.Touch.Views;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class BookingBarNormalBooking : MvxView
	{
		public static BookingBarNormalBooking LoadViewFromFile()
		{
			var bookingView = NSBundle.MainBundle.LoadNib("BookingBarNormalBooking", null, null).GetItem<BookingBarNormalBooking>(0);

			bookingView.buttonEstimate.Initialize(Localize.GetValue("Destination"), "destination_small_icon.png", "destination_small_icon_pressed.png");

			FlatButtonStyle.Green.ApplyTo(bookingView.buttonBooking);

			bookingView.buttonReservationBooking.Initialize(Localize.GetValue("BookItLaterButton"), "later_icon.png", "later_icon_pressed.png");

			return bookingView;
		}

		public BookingBarNormalBooking(IntPtr handle):base(handle)
		{
			this.DelayBind(DataBinding);
		}

		public void DataBinding()
		{
			var set = this.CreateBindingSet<BookingBarNormalBooking, BottomBarViewModel>();

			set.Bind()
				.For(v => v.Hidden)
				.To(vm => vm.HideOrderButtons);

			set.Bind(buttonEstimate)
				.For(v => v.Command)
				.To(vm => vm.ChangeAddressSelectionMode);
			set.Bind(buttonEstimate)
				.For(v => v.Selected)
				.To(vm => vm.EstimateSelected);
			set.Bind(viewEstimate)
				.For(v => v.Hidden)
				.To(vm => vm.Settings.HideDestination);

			set.Bind(buttonBooking)
				.For(v => v.Command)
				.To(vm => vm.Book);
			set.Bind(viewBooking)
				.For(v => v.Hidden)
				.To(vm => vm.BookButtonHidden);
			set.Bind(buttonBooking)
				.For("Title")
				.To(vm => vm.BookButtonText);

			set.Bind(buttonReservationBooking)
				.For(v => v.Command)
				.To(vm => vm.BookLater);
			set.Bind(viewReservationBooking)
				.For(v => v.Hidden)
				.To(vm => vm.IsFutureBookingDisabled);

			set.Bind(imagePromotion)
				.For(v => v.Hidden)
				.To(vm => vm.IsPromoCodeActive)
				.WithConversion("BoolInverter");

			set.Apply();
		}
	}
}