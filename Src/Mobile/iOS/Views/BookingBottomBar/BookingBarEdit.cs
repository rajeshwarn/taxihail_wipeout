using System;
using Foundation;
using UIKit;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.Client.Localization;
using Cirrious.MvvmCross.Binding.Touch.Views;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class BookingBarEdit : MvxView
	{
		public static BookingBarEdit LoadViewFromFile()
		{
			var bookingView = NSBundle.MainBundle.LoadNib("BookingBarEdit", null, null).GetItem<BookingBarEdit>(0);

			bookingView.buttonCancel.Initialize(Localize.GetValue("Cancel"));
            bookingView.buttonSave.SetTitle(Localize.GetValue("Save"), UIControlState.Normal);

			FlatButtonStyle.Green.ApplyTo(bookingView.buttonSave);
			
			return bookingView;
		}

		public BookingBarEdit(IntPtr handle):base(handle)
		{
			this.DelayBind(DataBinding);
		}

		public void DataBinding()
		{
			var set = this.CreateBindingSet<BookingBarEdit, BottomBarViewModel>();

			set.Bind()
				.For(v => v.Hidden)
				.To(vm => vm.HideEditButtons);

			set.Bind(buttonCancel)
                .For(v => v.Command)
				.To(vm => vm.CancelEdit);

			set.Bind(buttonSave)
				.For(v => v.Command)
				.To(vm => vm.Save);

			set.Apply();
		}
	}
}