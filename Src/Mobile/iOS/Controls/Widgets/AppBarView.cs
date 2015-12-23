using System;
using Foundation;
using UIKit;
using CoreGraphics;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;
using Cirrious.MvvmCross.Binding.Touch.Views;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.Client.Views;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.Client.Views.BookingBottomBar;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    [Register("AppBarView")]
    public class AppBarView : MvxView
    {
		private BookingBarNormalBooking _bookingBarNormalBooking;
		private BookingBarManualRideLinQ _bookingBarManualRideLinQ;
		private BookingBarAirportBooking _bookingBarAirportBooking;
		private BookingBarConfirmation _bookingBarConfirmation;
		private BookingBarEdit _bookingBarEdit;
		private BookingBarInTripNormalBooking _bookingBarInTripNormalBooking;
        private BookingBarInTripManualRideLinQBooking _bookingBarInTripManualRideLinQBooking;
        private BookingBarDropOffSelection _bookingBarDropOffSelection;

		protected UIView Line { get; set; }

        public AppBarView (IntPtr ptr):base(ptr)
        {
            Initialize ();
        }

        public AppBarView ()
        {
            Initialize ();
        }

        void Initialize ()
        {
            BackgroundColor = UIColor.White;

			_bookingBarNormalBooking = BookingBarNormalBooking.LoadViewFromFile();
			Add(_bookingBarNormalBooking);

			_bookingBarManualRideLinQ = BookingBarManualRideLinQ.LoadViewFromFile();
			Add(_bookingBarManualRideLinQ);

			_bookingBarAirportBooking = BookingBarAirportBooking.LoadViewFromFile();
			Add(_bookingBarAirportBooking);

			_bookingBarConfirmation = BookingBarConfirmation.LoadViewFromFile();
			Add(_bookingBarConfirmation);

			_bookingBarEdit = BookingBarEdit.LoadViewFromFile();
			Add(_bookingBarEdit);

			_bookingBarInTripNormalBooking = BookingBarInTripNormalBooking.LoadViewFromFile();
			Add(_bookingBarInTripNormalBooking);

			_bookingBarInTripManualRideLinQBooking = BookingBarInTripManualRideLinQBooking.LoadViewFromFile();
            Add(_bookingBarInTripManualRideLinQBooking);

            _bookingBarDropOffSelection = BookingBarDropOffSelection.LoadViewFromFile();
            Add(_bookingBarDropOffSelection);

            Line = new UIView
            {
                BackgroundColor = UIColor.FromRGB(140, 140, 140)
            };
            AddSubview(Line);

			this.DelayBind(DataBinding);
        }

		void DataBinding()
		{
			var setBooking = this.CreateBindingSet<AppBarView, HomeViewModel>();

			setBooking.Bind(_bookingBarNormalBooking).For(v => v.DataContext).To(vm => vm.BottomBar);
			setBooking.Bind(_bookingBarManualRideLinQ).For(v => v.DataContext).To(vm => vm.BottomBar);
			setBooking.Bind(_bookingBarAirportBooking).For(v => v.DataContext).To(vm => vm.BottomBar);
			setBooking.Bind(_bookingBarConfirmation).For(v => v.DataContext).To(vm => vm.BottomBar);
			setBooking.Bind(_bookingBarEdit).For(v => v.DataContext).To(vm => vm.BottomBar);
            setBooking.Bind(_bookingBarInTripNormalBooking).For(v => v.DataContext).To(vm => vm);
            setBooking.Bind(_bookingBarInTripManualRideLinQBooking).For(v => v.DataContext).To(vm => vm);
            setBooking.Bind(_bookingBarDropOffSelection).For(v => v.DataContext).To(vm => vm);

			setBooking.Apply();
		}

		public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            if (Line != null)
            {
                Line.Frame = new CGRect(0, 0, Frame.Width, UIHelper.OnePixel);
            }
        }
	}
}