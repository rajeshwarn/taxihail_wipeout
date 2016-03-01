using System;
using Foundation;
using UIKit;
using Cirrious.MvvmCross.Binding.Touch.Views;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class BookingBarInTripNormalBooking :  MvxView
	{
		public static BookingBarInTripNormalBooking LoadViewFromFile()
		{
			var bookingView = NSBundle.MainBundle.LoadNib("BookingBarInTripNormalBooking", null, null).GetItem<BookingBarInTripNormalBooking>(0);

			bookingView.BackgroundColor = UIColor.Clear;

            bookingView.buttonCall.SetTitle(Localize.GetValue("CallButton"), UIControlState.Normal);
            bookingView.buttonCancel.SetTitle(Localize.GetValue("StatusCancelButton"), UIControlState.Normal);
            
            FlatButtonStyle.Red.ApplyTo(bookingView.buttonCancel);
            FlatButtonStyle.Red.ApplyTo(bookingView.buttonUnpair);
            FlatButtonStyle.Silver.ApplyTo(bookingView.buttonCall);
            FlatButtonStyle.Silver.ApplyTo(bookingView.buttonEditTip);

			return bookingView;
		}

		public BookingBarInTripNormalBooking(IntPtr handle):base(handle)
		{
			this.DelayBind(DataBinding);
		}

		public void DataBinding()
		{
			var set = this.CreateBindingSet<BookingBarInTripNormalBooking, HomeViewModel>();

			set.Bind()
				.For(v => v.Hidden)
				.To(vm => vm.CurrentViewState)
				.WithConversion("EnumToInvertedBool", new[] { HomeViewModelState.BookingStatus });

			set.Bind(viewCancelEditTip)
				.For(v => v.HiddenWithConstraints)
				.To(vm => vm.BookingStatus.BottomBar.IsCancelOrTipChangeButtonsVisible)
                .WithConversion("BoolInverter");
            set.Bind(viewUnpairEditTip)
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
			set.Bind (buttonUnpair)
				.For ("Title")
				.To (vm => vm.BookingStatus.BottomBar.UnpairButtonText);
			
			set.Bind(buttonCancel)
				.For(v => v.Command)
				.To(vm => vm.BookingStatus.BottomBar.CancelOrder);
			set.Bind(buttonCancel)
				.For(v => v.HiddenWithConstraints)
				.To(vm => vm.BookingStatus.BottomBar.IsCancelButtonVisible)
				.WithConversion("BoolInverter");
            
			set.Bind(buttonCall)
				.For(v => v.Command)
				.To(vm => vm.BookingStatus.BottomBar.CallCompany);
			set.Bind(buttonCall)
				.For(v => v.HiddenWithConstraints)
				.To(vm => vm.BookingStatus.BottomBar.IsCallCompanyHidden);

			set.Bind(buttonEditTip)
				.For(v => v.HiddenWithConstraints)
				.To(vm => vm.BookingStatus.BottomBar.CanEditAutoTip)
				.WithConversion("BoolInverter");
			set.Bind(buttonEditTip)
				.For("Title")
				.To(vm => vm.BookingStatus.BottomBar.ButtonEditTipLabel);
			set.Bind(buttonEditTip)
				.For(v => v.Command)
				.To(vm => vm.BookingStatus.BottomBar.EditAutoTipCommand);

			set.Apply();
		}
	}
}