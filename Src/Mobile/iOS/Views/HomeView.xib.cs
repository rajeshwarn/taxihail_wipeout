using Cirrious.MvvmCross.Binding.BindingContext;
using UIKit;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets.Booking;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using apcurium.MK.Booking.Mobile.PresentationHints;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Mobile.Client.Style;
using System.Windows.Input;
using System;
using apcurium.MK.Booking.Mobile.Client.Localization;
using System.ComponentModel;
using System.Reactive.Linq;
using apcurium.MK.Booking.Mobile.Client.Diagnostics;
using System.Reactive.Disposables;
using Foundation;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
    public partial class HomeView : BaseViewController<HomeViewModel>, IChangePresentation
    {
        private BookLaterDatePicker _datePicker;
        private readonly SerialDisposable _viewStateSubscription = new SerialDisposable();
        private readonly SerialDisposable _bookingStatusContactDriverSubscription = new SerialDisposable();
        private readonly SerialDisposable _bookingStatusChangeDropOffSubscription = new SerialDisposable();
        private readonly SerialDisposable _orderStatusDetailSubscription = new SerialDisposable();

        private const int BookingStatusHiddenConstraintValue = -200;
        private const int ContactDriverHiddenConstraintValue = -283;
        private const int ContactDriverInTaxiHiddenConstraintValue = -70;
        private const int ChangeDropOffHiddenConstraintValue = -50;
        private const int BookingStatusAppBarHiddenConstraintValue = 80;
        private const int DropOffSelectionAppBarHiddenConstraintValue = 80;
        private const int BookingStatusHeight = 75;
        private const int BookingStatusAndDriverInfosHeight = 158;

	    private const int MarginBetweenOverlay = 16;

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
                _bookingStatusContactDriverSubscription.Disposable = ObserveIsContactTaxiVisible();
                _bookingStatusChangeDropOffSubscription.Disposable = ObserveIsChangeDropOffVisible();
                _orderStatusDetailSubscription.Disposable = ObserveIsDriverInfoAvailable();
            }
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            if (ViewModel != null)
            {
                ViewModel.UnsubscribeLifetimeChangedIfNecessary();
            }
        }

        public override void ViewDidUnload()
        {
            base.ViewDidUnload();
            UnregisterKeyboardNotifications();
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

        private IDisposable ObserveIsChangeDropOffVisible()
        {
            return Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                h => ViewModel.BookingStatus.PropertyChanged += h,
                h => ViewModel.BookingStatus.PropertyChanged -= h
            )
                .Where(args => args.EventArgs.PropertyName.Equals("IsChangeDropOffVisible"))
                .Select(_ => ViewModel.BookingStatus.IsChangeDropOffVisible)
                .DistinctUntilChanged()
                .Subscribe(ToggleChangeDropOffVisibility, Logger.LogError);
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

        protected override void KeyboardWillShowNotification(NSNotification notification)
        {
            if (ViewModel.CurrentViewState == HomeViewModelState.Initial)
            {
                View.ResignFirstResponderOnSubviews();
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            RegisterForKeyboardNotifications();

            btnMenu.SetImage(UIImage.FromFile("menu_icon.png"), UIControlState.Normal);

            btnLocateMe.SetImage(UIImage.FromFile("location_icon.png"), UIControlState.Normal);

            _datePicker = new BookLaterDatePicker();            
			_datePicker.UpdateView(UIScreen.MainScreen.Bounds.Height, UIScreen.MainScreen.Bounds.Width);
            _datePicker.Hide();
            View.AddSubview(_datePicker);

            panelMenu.ViewToAnimate = homeView;
            panelMenu.PanelOffsetConstraint = constraintHomeLeadingSpace;

            btnMenu.AccessibilityLabel = Localize.GetValue("MenuButton");
            btnLocateMe.AccessibilityLabel = Localize.GetValue("LocateMeButton");
            btnAirport.AccessibilityLabel = Localize.GetValue("AirportsButton");
            btnTrain.AccessibilityLabel = Localize.GetValue("TrainStationsButton");

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
                .WithConversion("EnumToBool", new[] { HomeViewModelState.BookingStatus, HomeViewModelState.ManualRidelinq});

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
                .WithConversion("EnumToBool", new[] { HomeViewModelState.BookingStatus, HomeViewModelState.ManualRidelinq });

            set.Bind(mapView)
                .For(v => v.DataContext)
                .To(vm => vm.Map);

            set.Bind(ctrlOrderOptions)
                .For(v => v.DataContext)
                .To(vm => vm.OrderOptions);

            set.Bind(ctrlDropOffSelection)
                .For(v => v.DataContext)
                .To(vm => vm.DropOffSelection);

            set.Bind(ctrlAddressPicker)
                .For(v => v.DataContext)
                .To(vm => vm.AddressPicker);

            set.Bind(ctrlOrderReview)
                .For(v => v.DataContext)
                .To(vm => vm.OrderReview);
                
            set.Bind(orderEdit)
                .For(v => v.DataContext)
                .To(vm => vm.OrderEdit);

			set.Bind (orderAirport)
				.For (v => v.DataContext)
				.To (vm => vm.OrderAirport);

            set.Bind(bottomBar)
                .For(v => v.DataContext)
                .To(vm => vm.BottomBar);

            set.Bind(_datePicker)
                .For(v => v.DataContext)
                .To(vm => vm.BottomBar);

			set.Bind(ctrlOrderBookingOptions)
				.For(v => v.DataContext)
				.To(vm => vm.BottomBar);

            set.Bind()
                .For(v => v.HomeViewState)
                .To(vm => vm.CurrentViewState);

            #region BookingStatus

            set.Bind(mapView)
                .For(v => v.MapCenter)
                .To(vm => vm.BookingStatus.MapCenter);

            set.Bind(bookingStatusBottomBar)
                .For(v => v.DataContext)
                .To(vm => vm.BookingStatus.BottomBar);

            set.Bind(dropOffSelectionBottomBar)
                .For(v => v.DataContext)
                .To(vm => vm.DropOffSelection.BottomBar);

            set.Bind(bookingStatusControl)
                .For(v => v.DataContext)
                .To(vm => vm.BookingStatus);

            set.Bind(mapView)
                .For(v => v.OrderStatusDetail)
                .To(vm => vm.BookingStatus.OrderStatusDetail);

            set.Bind(contactTaxiControl)
                .For(v => v.DataContext)
                .To(vm => vm.BookingStatus);

            set.Bind(changeDropOffControl)
                .For(v => v.DataContext)
                .To(vm => vm.BookingStatus);

	        set.Bind(mapView)
		        .For(v => v.TaxiLocation)
		        .To(vm => vm.BookingStatus.TaxiLocation);

            set.Bind(mapView)
                .For(v => v.CancelAutoFollow)
                .To(vm => vm.BookingStatus.CancelAutoFollow);

            mapView.OverlayOffsetProvider = GetOverlayOffset;
            
            #endregion

            set.Apply();
        }

        private HomeViewModelState _homeViewState;
        public HomeViewModelState HomeViewState
        {
            get
            {
                return _homeViewState;
            }
            set
            {
                _homeViewState = value;
                ChangeState(_homeViewState);
            }
        }


        private nfloat GetOverlayOffset()
        {
            var screenOffset = (nfloat)Math.Abs(UIScreen.MainScreen.ApplicationFrame.Height - UIScreen.MainScreen.Bounds.Height);

            var overlayOffset = bookingStatusControl.Bounds.Height + screenOffset;

            overlayOffset = ViewModel.BookingStatus.IsContactTaxiVisible 
                ? overlayOffset + contactTaxiControl.Bounds.Height + MarginBetweenOverlay
                : overlayOffset;
            
            overlayOffset = ViewModel.BookingStatus.IsChangeDropOffVisible 
                ? overlayOffset + changeDropOffControl.Bounds.Height + MarginBetweenOverlay
                : overlayOffset;

            return overlayOffset;
        }

        private void ToggleContactTaxiVisibility(bool isContactTaxiVisible)
        {
            if (isContactTaxiVisible && constraintContactTaxiTopSpace.Constant != 8f)
            {
                // Show
                UIView.Animate(
                    0.6f, 
                    () => constraintContactTaxiTopSpace.Constant = 8f,
                    () => 
                    {
                        RedrawSubViews();
                        homeView.LayoutIfNeeded();
                    });
            }
            else if (!isContactTaxiVisible && constraintContactTaxiTopSpace.Constant != ContactDriverHiddenConstraintValue)
            {
                // Hide
                UIView.Animate(
                    0.6f, 
                    () => constraintContactTaxiTopSpace.Constant = ContactDriverInTaxiHiddenConstrainValue,
                    () => 
                    {
                        RedrawSubViews();
                        homeView.LayoutIfNeeded();
                    });
            }
        }

        private void ToggleChangeDropOffVisibility(bool isChangeDropOffVisible)
        {
            if (isChangeDropOffVisible && constraintChangeDropOffTopSpace.Constant == ChangeDropOffHiddenConstraintValue)
            {
                // Show
                UIView.Animate(
                    0.6f, 
                    () =>
                    {
                        constraintChangeDropOffTopSpace.Constant = ViewModel.BookingStatus.IsContactTaxiVisible ? 76f : 8f;

                        homeView.LayoutIfNeeded();
                    },
                    RedrawSubViews);
            }
            else if (!isChangeDropOffVisible && constraintChangeDropOffTopSpace.Constant != ChangeDropOffHiddenConstraintValue)
            {
                // Hide
                UIView.Animate(
                    0.6f, 
                    () =>
                    {
                        constraintChangeDropOffTopSpace.Constant = ChangeDropOffHiddenConstraintValue;
                        changeDropOffControl.SetNeedsDisplay();

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

                _datePicker.ViewState = state;
                _datePicker.Show();
            }
            else if (state == HomeViewModelState.AirportPickDate)
            {
                _datePicker.ViewState = state;
                _datePicker.Show();
            }
            else if (state == HomeViewModelState.BookATaxi)
            {
                // nothing to do but we can't remove the condition 
                // otherwise it gets picked up in the "search mode" catch all
            }
            else if (state == HomeViewModelState.Initial)
            {
                // Order Options: Visible
                // Order Review: Hidden
                // Order Edit: Hidden
                // Date Picker: Hidden

                CloseBookATaxiDialog();
                constraintAppBarDropOffSelection.Constant = DropOffSelectionAppBarHiddenConstraintValue;
                constraintAppBarBookingStatus.Constant = BookingStatusAppBarHiddenConstraintValue;
                constraintContactTaxiTopSpace.Constant = ContactDriverHiddenConstraintValue;
                constraintChangeDropOffTopSpace.Constant = ChangeDropOffHiddenConstraintValue;
                constraintDropOffSelectionTopSpace.Constant = -ctrlDropOffSelection.Frame.Height - 200f;
                ctrlDropOffSelection.SetNeedsLayout();
                ctrlAddressPicker.Close();
                _datePicker.Hide();  

                UIView.Animate(
                    0.6f, 
                    () =>
                    {
                        bookingStatusTopSpaceConstraint.Constant = BookingStatusHiddenConstraintValue;

                        constraintOrderReviewTopSpace.Constant = UIScreen.MainScreen.Bounds.Height;
                        constraintOrderReviewBottomSpace.Constant = constraintOrderReviewBottomSpace.Constant + UIScreen.MainScreen.Bounds.Height;
                        constraintOrderOptionsTopSpace.Constant = 22;
                        constraintOrderEditTrailingSpace.Constant = UIScreen.MainScreen.Bounds.Width;
                        constraintOrderAirportTopSpace.Constant = UIScreen.MainScreen.Bounds.Height + 22;
                        constraintOrderAirportBottomSpace.Constant = constraintOrderAirportBottomSpace.Constant + UIScreen.MainScreen.Bounds.Height;


                    }, () =>
                    {
                        RedrawSubViews();
                        homeView.LayoutIfNeeded();
                        ctrlAddressPicker.ResignFirstResponderOnSubviews();
                    });
            }
            else if (state == HomeViewModelState.Review)
            {
                // Order Options: Visible
                // Order Review: Visible
                // Order Edit: Hidden
                // Date Picker: Hidden
                CloseBookATaxiDialog();
                _datePicker.Hide();

                UIView.Animate(
                    0.6f, 
                    () =>
                    {
                        constraintOrderReviewTopSpace.Constant = 10;
                        constraintOrderReviewBottomSpace.Constant = -65;
                        constraintOrderOptionsTopSpace.Constant = 22;
                        constraintOrderEditTrailingSpace.Constant = UIScreen.MainScreen.Bounds.Width;
                        constraintOrderAirportTopSpace.Constant = UIScreen.MainScreen.Bounds.Height + 22;
                        constraintOrderAirportBottomSpace.Constant = constraintOrderAirportBottomSpace.Constant + UIScreen.MainScreen.Bounds.Height;
                    },
                    () =>
                    {
                        RedrawSubViews();
                        homeView.LayoutIfNeeded();
                    });
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
                        constraintOrderAirportTopSpace.Constant = UIScreen.MainScreen.Bounds.Height + 22;
                        constraintOrderAirportBottomSpace.Constant = constraintOrderAirportBottomSpace.Constant + UIScreen.MainScreen.Bounds.Height;

                    }, () =>
                    {
                        homeView.LayoutIfNeeded();
                        ctrlOrderReview.SetNeedsDisplay();
                        ctrlOrderOptions.SetNeedsDisplay();
                    });
            }
            else if (state == HomeViewModelState.BookingStatus || state == HomeViewModelState.ManualRidelinq)
            {
                // Order Options: Hidden
                // Order Review: Hidden
                // Order Edit: Hidden
                // Date Picker: Hidden
                // Adress Picker: Hidden
                // Initial app bar: Hidden
                // Booking Status app bar: Visible

                CloseBookATaxiDialog();
                _bookingStatusContactDriverSubscription.Disposable = ObserveIsContactTaxiVisible();
                _bookingStatusChangeDropOffSubscription.Disposable = ObserveIsChangeDropOffVisible();
                constraintAppBarBookingStatus.Constant = 0;

                if (ViewModel.BookingStatus != null && !ViewModel.BookingStatus.IsContactTaxiVisible)
                {
                    constraintContactTaxiTopSpace.Constant = ContactDriverInTaxiHiddenConstraintValue;
                }
                if (ViewModel.BookingStatus != null && !ViewModel.BookingStatus.IsChangeDropOffVisible)
                {
                    constraintChangeDropOffTopSpace.Constant = ChangeDropOffHiddenConstraintValue;
                }
                if (ViewModel.BookingStatus != null)
                {
                    var isManualPairing = state == HomeViewModelState.ManualRidelinq;

                    ResizeBookingStatusControl(!isManualPairing && ViewModel.BookingStatus.IsDriverInfoAvailable);
                }

                ctrlAddressPicker.Close();
                _datePicker.Hide();

                UIView.Animate(
                    0.6f, 
                    () =>
                    {
                        
                        constraintOrderReviewTopSpace.Constant = UIScreen.MainScreen.Bounds.Height + 100f;
                        constraintAppBarDropOffSelection.Constant = DropOffSelectionAppBarHiddenConstraintValue;
                        constraintOrderReviewBottomSpace.Constant = constraintOrderReviewBottomSpace.Constant + UIScreen.MainScreen.Bounds.Height;
                        constraintOrderOptionsTopSpace.Constant = -ctrlOrderOptions.Frame.Height - 122f;
                        constraintOrderEditTrailingSpace.Constant = UIScreen.MainScreen.Bounds.Width;
                        constraintOrderAirportTopSpace.Constant = UIScreen.MainScreen.Bounds.Height + 122;
                        constraintOrderAirportBottomSpace.Constant = constraintOrderAirportBottomSpace.Constant + UIScreen.MainScreen.Bounds.Height;
                        bookingStatusTopSpaceConstraint.Constant = 22f;

                          
                    }, () => 
                    {
                        RedrawSubViews();
                        homeView.LayoutIfNeeded();
                    });
			}
            else if (state == HomeViewModelState.DropOffAddressSelection)
            {
                UIView.Animate(
                    0.6f, 
                    () =>
                    {
                        ctrlAddressPicker.Close();
                        constraintAppBarDropOffSelection.Constant = 0;
                        constraintAppBarBookingStatus.Constant = BookingStatusAppBarHiddenConstraintValue;
                        constraintChangeDropOffTopSpace.Constant = ChangeDropOffHiddenConstraintValue;
                        constraintContactTaxiTopSpace.Constant = ContactDriverInTaxiHiddenConstraintValue;
                        bookingStatusTopSpaceConstraint.Constant = BookingStatusHiddenConstraintValue;
                        constraintDropOffSelectionTopSpace.Constant = 22f;

                        ctrlDropOffSelection.SetNeedsLayout();
                    }, RedrawSubViews);
            }
            else if (state == HomeViewModelState.AirportDetails)
            {
				// Order Options: Hidden
				// Order Review: Hidden
				// Order Edit: Hidden
				// Date Picker: Hidden
				// Order Airport: Visable
				CloseBookATaxiDialog ();

                ctrlAddressPicker.Close ();
                _datePicker.Hide();
                 
				UIView.Animate(
					0.6f, 
					() =>
					{
						
						constraintOrderReviewTopSpace.Constant = UIScreen.MainScreen.Bounds.Height;
						constraintOrderReviewBottomSpace.Constant = constraintOrderReviewBottomSpace.Constant + UIScreen.MainScreen.Bounds.Height;
						constraintOrderOptionsTopSpace.Constant = 22;
						constraintOrderEditTrailingSpace.Constant = UIScreen.MainScreen.Bounds.Width;       
						constraintOrderAirportTopSpace.Constant = 10;
						constraintOrderAirportBottomSpace.Constant = -65;
					},
                    () => 
                    {
                        RedrawSubViews();
                        homeView.LayoutIfNeeded();
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
                        constraintDropOffSelectionTopSpace.Constant = -ctrlDropOffSelection.Frame.Height - 200f;
						constraintOrderAirportTopSpace.Constant = UIScreen.MainScreen.Bounds.Height + 22;
						constraintOrderAirportBottomSpace.Constant = constraintOrderAirportBottomSpace.Constant + UIScreen.MainScreen.Bounds.Height;
						
                    }, () => 
                    {
                        RedrawSubViews();
                        homeView.LayoutIfNeeded();
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
            ctrlDropOffSelection.SetNeedsDisplay();
			ctrlOrderBookingOptions.SetNeedsDisplay();
			orderAirport.SetNeedsDisplay ();
            bookingStatusBottomBar.SetNeedsDisplay();
            bookingStatusControl.SetNeedsDisplay();
            contactTaxiControl.SetNeedsDisplay();
            changeDropOffControl.SetNeedsDisplay();
        }

        public void ChangePresentation(ChangePresentationHint hint)
        {
            mapView.ChangePresentation(hint);
        }
    }
}