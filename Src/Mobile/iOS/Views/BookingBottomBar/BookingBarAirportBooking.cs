
using System;

using Foundation;
using UIKit;
using apcurium.MK.Booking.Mobile.Client.Controls.Binding;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.Client.Localization;
using Cirrious.MvvmCross.Binding.Touch.Views;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class BookingBarAirportBooking : MvxView
	{
		public static BookingBarAirportBooking LoadViewFromFile()
		{
			var bookingView = NSBundle.MainBundle.LoadNib("BookingBarAirportBooking", null, null).GetItem<BookingBarAirportBooking>(0);

			bookingView.buttonCancel.Initialize(Localize.GetValue("Cancel"));

			bookingView.buttonNextAirport.SetTitle(Localize.GetValue("Next"), UIControlState.Normal);
			FlatButtonStyle.Green.ApplyTo(bookingView.buttonNextAirport);

			return bookingView;
		}

		public BookingBarAirportBooking(IntPtr handle):base(handle)
		{
			this.DelayBind(DataBinding);
		}

		public void DataBinding()
		{
			var set = this.CreateBindingSet<BookingBarAirportBooking, BottomBarViewModel>();

			set.Bind(this).For(v => v.Hidden).To(vm => vm.HideAirportButtons);

			set.Bind(buttonCancel).For(v => v.Command).To(vm => vm.CancelAirport);

			set.Bind(buttonNextAirport).For(v => v.Command).To(vm => vm.NextAirport);

			set.Apply();
		}
	}
}