using System;
using UIKit;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using Cirrious.MvvmCross.Binding.BindingContext;
using Foundation;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.Touch.Views;

namespace apcurium.MK.Booking.Mobile.Client.Views.BookingBottomBar
{
    public partial class BookingBarDropOffSelection : MvxView
    {
        public static BookingBarDropOffSelection LoadViewFromFile()
        {
            var bookingView = NSBundle.MainBundle.LoadNib("BookingBarDropOffSelection", null, null).GetItem<BookingBarDropOffSelection>(0);

            bookingView.BackgroundColor = UIColor.Clear;

            FlatButtonStyle.Red.ApplyTo(bookingView.btnCancel);
            FlatButtonStyle.Green.ApplyTo(bookingView.btnOk);

            bookingView.Hidden = true;

            bookingView.btnCancel.SetTitle(Localize.GetValue("Cancel"), UIControlState.Normal);
            bookingView.btnOk.SetTitle(Localize.GetValue("OkButtonText"), UIControlState.Normal);

            return bookingView;
        }

        public BookingBarDropOffSelection(IntPtr handle):base(handle)
        {
            this.DelayBind(DataBinding);
        }

        private void DataBinding()
        {
            var set = this.CreateBindingSet<BookingBarDropOffSelection, HomeViewModel>();

            set.Bind()
                .For(v => v.Hidden)
                .To(vm => vm.CurrentViewState)
                .WithConversion("EnumToInvertedBool", new[] { HomeViewModelState.DropOffAddressSelection });

            set.Bind(btnCancel)
                .For(v => v.Command)
                .To(vm => vm.BottomBar.CancelChangeDropOff);

            set.Bind(btnOk)
                .For(v => v.Command)
                .To(vm => vm.BottomBar.SaveDropOff);

            set.Apply();
        }
    }
}


