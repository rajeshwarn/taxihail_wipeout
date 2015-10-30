using System;
using Foundation;
using UIKit;
using CoreGraphics;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using Cirrious.MvvmCross.Binding.Touch.Views;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using apcurium.MK.Common.Configuration;
using Cirrious.CrossCore;
using apcurium.MK.Booking.Mobile.Client.Views;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    [Register("AppBarView")]
    public class AppBarView : MvxView
    {
        protected UIView Line { get; set; }

        public AppBarView (IntPtr ptr):base(ptr)
        {
            Initialize ();
        }

        public AppBarView ()
        {
            Initialize ();
        }

		BookingBarNormalBooking _bookingBarNormalBooking;
		BookingBarManualRideLinQ _bookingBarManualRideLinQ;
		BookingBarAirportBooking _bookingBarAirportBooking;
		BookingBarConfirmation _bookingBarConfirmation;
		BookingBarEdit _bookingBarEdit;


        void Initialize ()
        {
            BackgroundColor = UIColor.White;

            Line = new UIView()
            {
                BackgroundColor = UIColor.FromRGB(140, 140, 140)
            };

            AddSubview(Line);

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

			this.DelayBind(DataBinding);
        }

		void DataBinding()
		{
			var set = this.CreateBindingSet<AppBarView, BottomBarViewModel>();

			set.Bind(_bookingBarNormalBooking).For(v => v.DataContext).To(vm => vm);
			set.Bind(_bookingBarManualRideLinQ).For(v => v.DataContext).To(vm => vm);
			set.Bind(_bookingBarAirportBooking).For(v => v.DataContext).To(vm => vm);
			set.Bind(_bookingBarConfirmation).For(v => v.DataContext).To(vm => vm);
			set.Bind(_bookingBarEdit).For(v => v.DataContext).To(vm => vm);

			set.Apply();
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