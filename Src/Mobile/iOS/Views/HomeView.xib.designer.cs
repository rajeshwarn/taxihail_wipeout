// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	[Register ("HomeView")]
	partial class HomeView
	{
		[Outlet]
		UIKit.NSLayoutConstraint _constraintOrderBookinOptionsTopSpace { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.BookingStatusControl bookingStatusControl { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint bookingStatusTopSpaceConstraint { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.AppBarView bottomBar { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.OverlayButton btnAirport { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.OverlayButton btnLocateMe { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.OverlayButton btnMenu { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.OverlayButton btnTrain { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.ChangeDropOffControl changeDropOffControl { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint constraintAppBarBookingStatus { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint constraintAppBarBookingStatusTopSpace { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint constraintAppBarBookingStatusVerticalSpace { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint constraintAppBarStatusBottomSpace { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint constraintBookingStatusHeight { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint constraintChangeDropOffTopSpace { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint constraintContactTaxiTopSpace { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint constraintDropOffSelectionTopSpace { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint constraintHomeLeadingSpace { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint constraintOrderAirportBottomSpace { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint constraintOrderAirportTopSpace { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint constraintOrderBookinOptionsTopSpace { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint constraintOrderEditTrailingSpace { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint constraintOrderOptionsTopSpace { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint constraintOrderReviewBottomSpace { get; set; }

		[Outlet] 
		UIKit.NSLayoutConstraint constraintOrderReviewTopSpace { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.ContactTaxiControl contactTaxiControl { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Views.AddressPicker.AddressPickerView ctrlAddressPicker { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.DropOffSelectionControl ctrlDropOffSelection { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Views.Order.OrderBookingOptions ctrlOrderBookingOptions { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.OrderOptionsControl ctrlOrderOptions { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Views.Order.OrderReviewView ctrlOrderReview { get; set; }

		[Outlet]
		UIKit.UIView homeView { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Views.OrderMapView mapView { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Views.Order.OrderAirportView orderAirport { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Views.Order.OrderEditView orderEdit { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Views.PanelMenuView panelMenu { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (_constraintOrderBookinOptionsTopSpace != null) {
				_constraintOrderBookinOptionsTopSpace.Dispose ();
				_constraintOrderBookinOptionsTopSpace = null;
			}

			if (bookingStatusControl != null) {
				bookingStatusControl.Dispose ();
				bookingStatusControl = null;
			}

			if (bookingStatusTopSpaceConstraint != null) {
				bookingStatusTopSpaceConstraint.Dispose ();
				bookingStatusTopSpaceConstraint = null;
			}

			if (bottomBar != null) {
				bottomBar.Dispose ();
				bottomBar = null;
			}

			if (btnAirport != null) {
				btnAirport.Dispose ();
				btnAirport = null;
			}

			if (btnLocateMe != null) {
				btnLocateMe.Dispose ();
				btnLocateMe = null;
			}

			if (btnMenu != null) {
				btnMenu.Dispose ();
				btnMenu = null;
			}

			if (btnTrain != null) {
				btnTrain.Dispose ();
				btnTrain = null;
			}

			if (changeDropOffControl != null) {
				changeDropOffControl.Dispose ();
				changeDropOffControl = null;
			}

			if (constraintAppBarBookingStatus != null) {
				constraintAppBarBookingStatus.Dispose ();
				constraintAppBarBookingStatus = null;
			}

			if (constraintAppBarBookingStatusTopSpace != null) {
				constraintAppBarBookingStatusTopSpace.Dispose ();
				constraintAppBarBookingStatusTopSpace = null;
			}

			if (constraintAppBarBookingStatusVerticalSpace != null) {
				constraintAppBarBookingStatusVerticalSpace.Dispose ();
				constraintAppBarBookingStatusVerticalSpace = null;
			}

			if (constraintAppBarStatusBottomSpace != null) {
				constraintAppBarStatusBottomSpace.Dispose ();
				constraintAppBarStatusBottomSpace = null;
			}

			if (constraintBookingStatusHeight != null) {
				constraintBookingStatusHeight.Dispose ();
				constraintBookingStatusHeight = null;
			}

			if (constraintChangeDropOffTopSpace != null) {
				constraintChangeDropOffTopSpace.Dispose ();
				constraintChangeDropOffTopSpace = null;
			}

			if (constraintContactTaxiTopSpace != null) {
				constraintContactTaxiTopSpace.Dispose ();
				constraintContactTaxiTopSpace = null;
			}

			if (constraintDropOffSelectionTopSpace != null) {
				constraintDropOffSelectionTopSpace.Dispose ();
				constraintDropOffSelectionTopSpace = null;
			}

			if (constraintHomeLeadingSpace != null) {
				constraintHomeLeadingSpace.Dispose ();
				constraintHomeLeadingSpace = null;
			}

			if (constraintOrderAirportBottomSpace != null) {
				constraintOrderAirportBottomSpace.Dispose ();
				constraintOrderAirportBottomSpace = null;
			}

			if (constraintOrderAirportTopSpace != null) {
				constraintOrderAirportTopSpace.Dispose ();
				constraintOrderAirportTopSpace = null;
			}

			if (constraintOrderBookinOptionsTopSpace != null) {
				constraintOrderBookinOptionsTopSpace.Dispose ();
				constraintOrderBookinOptionsTopSpace = null;
			}

			if (constraintOrderEditTrailingSpace != null) {
				constraintOrderEditTrailingSpace.Dispose ();
				constraintOrderEditTrailingSpace = null;
			}

			if (constraintOrderOptionsTopSpace != null) {
				constraintOrderOptionsTopSpace.Dispose ();
				constraintOrderOptionsTopSpace = null;
			}

			if (constraintOrderReviewBottomSpace != null) {
				constraintOrderReviewBottomSpace.Dispose ();
				constraintOrderReviewBottomSpace = null;
			}

			if (constraintOrderReviewTopSpace != null) {
				constraintOrderReviewTopSpace.Dispose ();
				constraintOrderReviewTopSpace = null;
			}

			if (contactTaxiControl != null) {
				contactTaxiControl.Dispose ();
				contactTaxiControl = null;
			}

			if (ctrlAddressPicker != null) {
				ctrlAddressPicker.Dispose ();
				ctrlAddressPicker = null;
			}

			if (ctrlDropOffSelection != null) {
				ctrlDropOffSelection.Dispose ();
				ctrlDropOffSelection = null;
			}

			if (ctrlOrderBookingOptions != null) {
				ctrlOrderBookingOptions.Dispose ();
				ctrlOrderBookingOptions = null;
			}

			if (ctrlOrderOptions != null) {
				ctrlOrderOptions.Dispose ();
				ctrlOrderOptions = null;
			}

			if (ctrlOrderReview != null) {
				ctrlOrderReview.Dispose ();
				ctrlOrderReview = null;
			}

			if (homeView != null) {
				homeView.Dispose ();
				homeView = null;
			}

			if (mapView != null) {
				mapView.Dispose ();
				mapView = null;
			}

			if (orderAirport != null) {
				orderAirport.Dispose ();
				orderAirport = null;
			}

			if (orderEdit != null) {
				orderEdit.Dispose ();
				orderEdit = null;
			}

			if (panelMenu != null) {
				panelMenu.Dispose ();
				panelMenu = null;
			}
		}
	}
}
