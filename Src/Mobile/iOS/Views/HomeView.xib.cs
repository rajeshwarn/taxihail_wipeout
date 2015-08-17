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
        private readonly SerialDisposable _subscription = new SerialDisposable();

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
                _subscription.Disposable = ObserveCurrentViewState();
            }
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            if (ViewModel != null)
            {
                ViewModel.UnsubscribeLifetimeChangedIfNecessary ();
                _subscription.Disposable = null;
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

            set.Bind(bookingStatusBottomBar)
                .For(v => v.HiddenWithConstraints)
                .To(vm => vm.CurrentViewState)
                .WithConversion("EnumToBool",
                    new[]
                    {
                        HomeViewModelState.Initial,
                        HomeViewModelState.BookATaxi,
                        HomeViewModelState.Edit,
                        HomeViewModelState.PickDate,
                        HomeViewModelState.Review,
                        HomeViewModelState.TrainStationSearch,
                        HomeViewModelState.AddressSearch,
                        HomeViewModelState.AirportSearch
                    });

            set.Bind(bookingStatusBottomBar)
                .For(v => v.DataContext)
                .To(vm => vm.BookingStatus.BottomBar);

            set.Bind(bookingStatusControl)
                .For(v => v.DataContext)
                .To(vm => vm.BookingStatus);

            /*set.Bind(bookingStatusControl)
                .For(v => v.HiddenWithConstraints)
                .To(vm => vm.CurrentViewState)
                .WithConversion("EnumToBool",
                    new[]
                    {
                        HomeViewModelState.Initial,
                        HomeViewModelState.BookATaxi,
                        HomeViewModelState.Edit,
                        HomeViewModelState.PickDate,
                        HomeViewModelState.Review,
                        HomeViewModelState.TrainStationSearch,
                        HomeViewModelState.AddressSearch,
                        HomeViewModelState.AirportSearch
                    });*/

            set.Bind(this.mapView)
                .For(v => v.TaxiLocation)
                .To(vm => vm.BookingStatus.OrderStatusDetail);

            #endregion

            set.Apply();
        }

        void ChangeState(HomeViewModelState state)
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

                UIView.Animate(
                    0.6f, 
                    () =>
                    {
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
        }

        public void ChangePresentation(ChangePresentationHint hint)
        {
            mapView.ChangePresentation(hint);
        }
    }
}