
using System;

using Foundation;
using UIKit;
using Cirrious.MvvmCross.Binding.Touch.Views;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class BookingBarInTripManualRideLinQBooking : MvxView
	{
		public static BookingBarInTripManualRideLinQBooking LoadViewFromFile()
		{
			var bookingView = NSBundle.MainBundle.LoadNib("BookingBarInTripManualRideLinQBooking", null, null).GetItem<BookingBarInTripManualRideLinQBooking>(0);

			bookingView.BackgroundColor = UIColor.Clear;

			FlatButtonStyle.Red.ApplyTo(bookingView.buttonUnpair);
			FlatButtonStyle.Silver.ApplyTo(bookingView.buttonCall);
			FlatButtonStyle.Silver.ApplyTo(bookingView.buttonTipChange);

			bookingView.Hidden = true;

			bookingView.buttonUnpair.SetTitle(Localize.GetValue("UnpairPayInCar"), UIControlState.Normal);
			bookingView.buttonCall.SetTitle(Localize.GetValue("CallButton"), UIControlState.Normal);

			return bookingView;
		}

		public BookingBarInTripManualRideLinQBooking(IntPtr handle):base(handle)
		{
			this.DelayBind(DataBinding);
		}

		public void DataBinding()
		{
			var set = this.CreateBindingSet<BookingBarInTripManualRideLinQBooking, HomeViewModel>();

			set.Bind()
				.For(v => v.Hidden)
				.To(vm => vm.CurrentViewState)
				.WithConversion("EnumToInvertedBool", new[] { HomeViewModelState.ManualRidelinq });

			set.Bind(viewUnpairTipChange)
				.For(v => v.HiddenWithConstraints)
				.To(vm => vm.BookingStatus.BottomBar.IsUnpairOrTipChangeButtonsVisible)
				.WithConversion("BoolInverter");

			set.Bind(buttonUnpair)
				.For(v => v.Command)
				.To(vm => vm.BookingStatus.BottomBar.Unpair);
			set.Bind(buttonUnpair)
				.For(v => v.HiddenWithConstraints)
				.To(vm => vm.BookingStatus.BottomBar.IsUnpairButtonVisible)
				.WithConversion("BoolInverter");

			set.Bind(buttonCall)
				.For(v => v.Command)
				.To(vm => vm.BookingStatus.BottomBar.CallCompany);
			set.Bind(buttonCall)
				.For(v => v.HiddenWithConstraints)
				.To(vm => vm.BookingStatus.BottomBar.IsCallCompanyHidden);

			set.Bind(buttonTipChange)
				.For(v => v.HiddenWithConstraints)
				.To(vm => vm.BookingStatus.BottomBar.CanEditAutoTip)
				.WithConversion("BoolInverter");
			set.Bind(buttonTipChange)
				.For("Title")
				.To(vm => vm.BookingStatus.BottomBar.ButtonEditTipLabel);
			set.Bind(buttonTipChange)
				.For(v => v.Command)
				.To(vm => vm.BookingStatus.BottomBar.EditAutoTipCommand);

			set.Apply();
		}
	}
}