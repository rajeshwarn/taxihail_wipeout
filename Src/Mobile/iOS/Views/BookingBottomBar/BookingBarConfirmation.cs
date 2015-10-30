
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
	public partial class BookingBarConfirmation : MvxView
	{
		public static BookingBarConfirmation LoadViewFromFile()
		{
			var bookingView = NSBundle.MainBundle.LoadNib("BookingBarConfirmation", null, null).GetItem<BookingBarConfirmation>(0);

			bookingView.buttonCancel.Initialize(Localize.GetValue("Cancel"));

			bookingView.buttonEdit.Initialize(Localize.GetValue("Edit"));

			FlatButtonStyle.Green.ApplyTo(bookingView.buttonConfirm);
			bookingView.buttonConfirm.SetTitle(Localize.GetValue("Confirm"), UIControlState.Normal);

			return bookingView;
		}

		public BookingBarConfirmation(IntPtr handle):base(handle)
		{
			this.DelayBind(DataBinding);
		}

		public void DataBinding()
		{
			var set = this.CreateBindingSet<BookingBarConfirmation, BottomBarViewModel>();

			set.Bind(this).For(v => v.Hidden).To(vm => vm.HideReviewButtons);

			set.Bind(buttonCancel).For(v => v.Command).To(vm => vm.CancelReview);

			set.Bind(buttonConfirm).For(v => v.Command).To(vm => vm.ConfirmOrderCommand);

			set.Bind(buttonEdit).For(v => v.Command).To(vm => vm.Edit);

			set.Apply();
		}
	}
}