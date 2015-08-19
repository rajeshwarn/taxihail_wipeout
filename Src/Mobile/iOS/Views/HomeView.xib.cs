using Cirrious.MvvmCross.Binding.BindingContext;
using UIKit;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets.Booking;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using apcurium.MK.Booking.Mobile.PresentationHints;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Mobile.Client.Style;
using System;
using System.ComponentModel;
using System.Reactive.Linq;
using apcurium.MK.Booking.Mobile.Client.Diagnostics;
using System.Reactive.Disposables;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
    public partial class HomeView : BaseViewController<HomeViewModel>, IChangePresentation
    {
        private BookLaterDatePicker _datePicker;
        private readonly SerialDisposable _viewStatesubscription = new SerialDisposable();
        private readonly SerialDisposable _bookingStatussubscription = new SerialDisposable();
        private readonly SerialDisposable _orderStatusDetailSubscription = new SerialDisposable();

        private const int BookingStatusHiddenConstraintValue = -200;
        private const int ContactDriverHiddenConstraintValue = -250;
        private const int BookingStatusAppBarHiddenConstrainValue = 80;
        private const int BookingStatusHeight = 75;
        private const int BookingStatusAndDriverInfosHeight = 158;


        public override void ViewWillAppear (bool animated)
        {
            base.ViewWillAppear (animated);

            if (!Theme.IsApplied)
            {
                // reset to default theme for the navigation bar
                ChangeThemeOfNavigationBar();
                Theme.IsApplied = true;
            }

            NavigationController.NavigationBar.BarStyle = UIBarStyle.Default;
            NavigationController.NavigationBar.Hidden = true;

            if (ViewModel != null)
            {
                ViewModel.SubscribeLifetimeChangedIfNecessary ();
                _viewStatesubscription.Disposable = ObserveCurrentViewState();
                _bookingStatussubscription.Disposable = ObserveIsContactTaxiVisible();
                _orderStatusDetailSubscription.Disposable = ObserveIsDriverInfoAvailable();
            }
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            if (ViewModel != null)
            {
                ViewModel.UnsubscribeLifetimeChangedIfNecessary();

                _viewStatesubscription.Dispose();
                _bookingStatussubscription.Dispose();
                _orderStatusDetailSubscription.Dispose();
            }
        }

        private IDisposable ObserveCurrentViewState()
        {
            return Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                h => ViewModel.PropertyChanged += h,
                h => ViewModel.PropertyChanged -= h
            )
                .Where(args => args.EventArgs.PropertyName.Equals("CurrentViewState"))
                .Select(_ => ViewModel.CurrentViewState)
                .DistinctUntilChanged()
                .Subscribe(ChangeState, Logger.LogError);
        }

        private IDisposable ObserveIsContactTaxiVisible()
        {
            return Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                h => ViewModel.BookingStatus.PropertyChanged += h,
                h => ViewModel.BookingStatus.PropertyChanged -= h
            )
                .Where(args => args.EventArgs.PropertyName.Equals("IsContactTaxiVisible"))
                .Select(_ => ViewModel.BookingStatus.IsContactTaxiVisible)
                .DistinctUntilChanged()
                .Subscribe(ToggleContactTaxiVisibility, Logger.LogError);
        }

        private IDisposable ObserveIsDriverInfoAvailable()
        {
            return Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                h => ViewModel.BookingStatus.PropertyChanged += h,
                h => ViewModel.BookingStatus.PropertyChanged -= h
            )
                .Where(args => args.EventArgs.PropertyName.Equals("IsDriverInfoAvailable"))
                .Select(_ => ViewModel.BookingStatus.IsDriverInfoAvailable)
                .DistinctUntilChanged()
                .Subscribe(ResizeBookingStatusControl, Logger.LogError);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            btnMenu.SetImage(UIImage.FromFile("menu_icon.png"), UIControlState.Normal);

            btnLocateMe.SetImage(UIImage.FromFile("location_icon.png"), UIControlState.Normal);

            _datePicker = new BookLaterDatePicker();            
			_datePicker.UpdateView(UIScreen.MainScreen.Bounds.Height, UIScreen.MainScreen.Bounds.Width);
            _datePicker.Hide();
            View.AddSubview(_datePicker);

            panelMenu.ViewToAnimate = homeView;
            panelMenu.PanelOffsetConstraint = constraintHomeLeadingSpace;

            var set = this.CreateBindingSet<HomeView, HomeViewModel>();

            set.Bind(panelMenu)
                .For(v => v.DataContext)
                .To(vm => vm.Panel);

            set.Bind(btnMenu)
                .For(v => v.Command)
                .To(vm => vm.Panel.OpenOrCloseMenu);

            set.Bind(btnMenu)
                .For(v => v.Hidden)
                .To(vm => vm.CurrentViewState)
                .WithConversion("EnumToBool", new[] { HomeViewModelState.BookingStatus });

			set.Bind(btnAirport)
				.For(v => v.Command)
				.To(vm => vm.AirportSearch);

            set.Bind(btnAirport)
                .For(v => v.Hidden)
                .To(vm => vm.IsAirportButtonHidden);

			set.Bind(btnTrain)
				.For(v => v.Command)
                .To(vm => vm.TrainStationSearch);

			set.Bind(btnTrain)
				.For(v => v.Hidden)
                .To(vm => vm.IsTrainButtonHidden);

            set.Bind(btnLocateMe)
                .For(v => v.Command)
                .To(vm => vm.LocateMe);

            set.Bind(btnLocateMe)
                .For(v => v.Hidden)
                .To(vm => vm.CurrentViewState)
                .WithConversion("EnumToBool", new[] { HomeViewModelState.BookingStatus });

            set.Bind(mapView)
                .For(v => v.DataContext)
                .To(vm => vm.Map);

            set.Bind(ctrlOrderOptions)
                .For(v => v.DataContext)
                .To(vm => vm.OrderOptions);

            set.Bind(ctrlAddressPicker)
                .For(v => v.DataContext)
                .To(vm => vm.AddressPicker);

            set.Bind(ctrlOrderReview)
                .For(v => v.DataContext)
                .To(vm => vm.OrderReview);
                
            set.Bind(orderEdit)
                .For(v => v.DataContext)
                .To(vm => vm.OrderEdit);

            set.Bind(bottomBar)
                .For(v => v.DataContext)
                .To(vm => vm.BottomBar);

            set.Bind(_datePicker)
                .For(v => v.DataContext)
                .To(vm => vm.BottomBar);

			set.Bind(ctrlOrderBookingOptions)
				.For(v => v.DataContext)
				.To(vm => vm.BottomBar);

            #region BookingStatus

            set.Bind(mapView)
                .For(v => v.MapCenter)
                .To(vm => vm.BookingStatus.MapCenter);

            set.Bind(bookingStatusBottomBar)
                .For(v => v.DataContext)
                .To(vm => vm.BookingStatus.BottomBar);

            set.Bind(bookingStatusControl)
                .For(v => v.DataContext)
                .To(vm => vm.BookingStatus);

            set.Bind(this.mapView)
                .For(v => v.OrderStatusDetail)
                .To(vm => vm.BookingStatus.OrderStatusDetail);

            set.Bind(contactTaxiControl)
                .For(v => v.DataContext)
                .To(vm => vm.BookingStatus);
            
            #endregion

            set.Apply();
        }

        private void ToggleContactTaxiVisibility(bool isContactTaxiVisible)
        {
            if (isContactTaxiVisible && constraintContactTaxiTopSpace.Constant != 8f)
            {
                // Show
                UIView.Animate(
                    0.6f, 
                    () =>
                    {
                        constraintContactTaxiTopSpace.Constant = 8f;

                        homeView.LayoutIfNeeded();
                    },
                    RedrawSubViews);
            }
            else if (!isContactTaxiVisible && constraintContactTaxiTopSpace.Constant != ContactDriverHiddenConstraintValue)
            {
                // Hide
                UIView.Animate(
                    0.6f, 
                    () =>
                    {
                        constraintContactTaxiTopSpace.Constant = /*ContactDriverHiddenConstraintValue*/-60;
                        contactTaxiControl.SetNeedsDisplay();

                        homeView.LayoutIfNeeded();
                    },
                    RedrawSubViews);
            }
        }

        private void ResizeBookingStatusControl(bool isDriverInfoAvailable)
        {
            if (isDriverInfoAvailable)
            {
                constraintBookingStatusHeight.Constant = BookingStatusAndDriverInfosHeight;
            }
            else
            {
                constraintBookingStatusHeight.Constant = BookingStatusHeight;
            }
            bookingStatusControl.SetNeedsDisplay();
        }

        private void ChangeState(HomeViewModelState state)
        {
			if (state == HomeViewModelState.PickDate)
			{
				// Order Options: Visible
				// Order Review: Hidden
				// Order Edit: Hidden
				// Date Picker: Visible

				CloseBookATaxiDialog();

				_datePicker.Show();
			}
			else if (state == HomeViewModelState.Review)
			{
				// Order Options: Visible
				// Order Review: Visible
				// Order Edit: Hidden
				// Date Picker: Hidden
				CloseBookATaxiDialog();

				UIView.Animate(
					0.6f, 
					() =>
                    {
						orderEdit.SetNeedsDisplay();
						ctrlOrderBookingOptions.SetNeedsDisplay();
						constraintOrderReviewTopSpace.Constant = 10;
						constraintOrderReviewBottomSpace.Constant = -65;
						constraintOrderOptionsTopSpace.Constant = 22;
						constraintOrderEditTrailingSpace.Constant = UIScreen.MainScreen.Bounds.Width;       

						_datePicker.Hide();
                        
                        homeView.LayoutIfNeeded();
					},
					RedrawSubViews);
			}
			else if (state == HomeViewModelState.BookATaxi)
			{
				constraintOrderBookinOptionsTopSpace.Constant = 0;

				homeView.LayoutIfNeeded();

				UIView.Animate (
					0.2f, 
					() => {
						ctrlOrderBookingOptions.Alpha = 1;
						homeView.LayoutIfNeeded(); 
					},
					RedrawSubViews);
			}
			else if (state == HomeViewModelState.Edit)
			{
				// Order Options: Hidden
				// Order Review: Hidden
				// Order Edit: Visible
				// Date Picker: Hidden
				UIView.Animate(
					0.6f, 
					() =>
					{
						constraintOrderReviewTopSpace.Constant = UIScreen.MainScreen.Bounds.Height;
						constraintOrderReviewBottomSpace.Constant = constraintOrderReviewBottomSpace.Constant + UIScreen.MainScreen.Bounds.Height;
						constraintOrderOptionsTopSpace.Constant = -ctrlOrderOptions.Frame.Height - 23f;
						constraintOrderEditTrailingSpace.Constant = 8;
						homeView.LayoutIfNeeded();
						ctrlOrderReview.SetNeedsDisplay();
						ctrlOrderOptions.SetNeedsDisplay();
					}, () => orderEdit.SetNeedsDisplay());
			}
			else if (state == HomeViewModelState.Initial)
			{
				// Order Options: Visible
				// Order Review: Hidden
				// Order Edit: Hidden
				// Date Picker: Hidden

				CloseBookATaxiDialog();
                constraintAppBarBookingStatus.Constant = BookingStatusAppBarHiddenConstrainValue;
                constraintContactTaxiTopSpace.Constant = ContactDriverHiddenConstraintValue;
                homeView.LayoutIfNeeded();

                UIView.Animate(
                    0.6f, 
                    () =>
                    {
                        bookingStatusTopSpaceConstraint.Constant = BookingStatusHiddenConstraintValue;

                        ctrlOrderReview.SetNeedsDisplay();
                        ctrlAddressPicker.Close();
                        constraintOrderReviewTopSpace.Constant = UIScreen.MainScreen.Bounds.Height;
                        constraintOrderReviewBottomSpace.Constant = constraintOrderReviewBottomSpace.Constant + UIScreen.MainScreen.Bounds.Height;
                        constraintOrderOptionsTopSpace.Constant = 22;
                        constraintOrderEditTrailingSpace.Constant = UIScreen.MainScreen.Bounds.Width;
                        homeView.LayoutIfNeeded();
                        _datePicker.Hide();  
                    }, () =>
                    {
                        RedrawSubViews();
                    });
			}
            else if (state == HomeViewModelState.BookingStatus)
            {
                // Order Options: Hidden
                // Order Review: Hidden
                // Order Edit: Hidden
                // Date Picker: Hidden
                // Adress Picker: Hidden
                // Initial app bar: Hidden
                // Booking Status app bar: Visible

                CloseBookATaxiDialog();
                constraintAppBarBookingStatus.Constant = 0;
                //constraintContactTaxiTopSpace.Constant = -58;

                homeView.LayoutIfNeeded();


                UIView.Animate(
                    0.6f, 
                    () =>
                    {
                        ctrlOrderReview.SetNeedsDisplay();
                        ctrlAddressPicker.Close();
                        constraintOrderReviewTopSpace.Constant = UIScreen.MainScreen.Bounds.Height;
                        constraintOrderReviewBottomSpace.Constant = constraintOrderReviewBottomSpace.Constant + UIScreen.MainScreen.Bounds.Height;
                        constraintOrderOptionsTopSpace.Constant = -ctrlOrderOptions.Frame.Height - 23f;
                        constraintOrderEditTrailingSpace.Constant = UIScreen.MainScreen.Bounds.Width;
                        bookingStatusTopSpaceConstraint.Constant = 22f;

                        homeView.LayoutIfNeeded();
                        _datePicker.Hide();  
                    }, () =>
                    {
                        RedrawSubViews();
                    });

            }
			// We consider any other options as one of the search options.
			else 
			{
				// Order Options: Hidden
				UIView.Animate(
					0.6f, 
					() =>
					{
						constraintOrderOptionsTopSpace.Constant = -ctrlOrderOptions.Frame.Height - 23f;
						homeView.LayoutIfNeeded();
					}, () =>
					{
						RedrawSubViews();
					});
				switch (state)
				{
					case HomeViewModelState.AddressSearch:
						ctrlAddressPicker.Open(AddressLocationType.Unspeficied);
						break;
					case HomeViewModelState.AirportSearch:
						ctrlAddressPicker.Open(AddressLocationType.Airport);
						break;
					case HomeViewModelState.TrainStationSearch:
						ctrlAddressPicker.Open(AddressLocationType.Train);
						break;
				}
			}
        }

		private void CloseBookATaxiDialog()
		{
			UIView.Animate (
				0.2f, 
				() => {
					ctrlOrderBookingOptions.Alpha = 0;
					homeView.LayoutIfNeeded ();
				},
				() =>
				{
					constraintOrderBookinOptionsTopSpace.Constant = UIScreen.MainScreen.Bounds.Height;
					RedrawSubViews();
				});
		}

		private void RedrawSubViews()
        {
            //redraw the shadows of the controls
            ctrlOrderReview.SetNeedsDisplay();
            ctrlOrderReview.SetNeedsLayout();
            orderEdit.SetNeedsDisplay();
            ctrlOrderOptions.SetNeedsDisplay();
			ctrlOrderBookingOptions.SetNeedsDisplay();
            bookingStatusBottomBar.SetNeedsDisplay();
            bookingStatusControl.SetNeedsDisplay();
            contactTaxiControl.SetNeedsDisplay();
        }

        public void ChangePresentation(ChangePresentationHint hint)
        {
            mapView.ChangePresentation(hint);
        }
    }
}