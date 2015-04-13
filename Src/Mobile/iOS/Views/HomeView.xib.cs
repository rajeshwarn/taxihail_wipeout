using Cirrious.MvvmCross.Binding.BindingContext;
using UIKit;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets.Booking;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using apcurium.MK.Booking.Mobile.PresentationHints;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Mobile.Client.Style;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
    public partial class HomeView : BaseViewController<HomeViewModel>, IChangePresentation
    {
        private BookLaterDatePicker _datePicker;

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
            }
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            if (ViewModel != null)
            {
                ViewModel.UnsubscribeLifetimeChangedIfNecessary ();
            }
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

			FlatButtonStyle.Green.ApplyTo(btnRideLinqCheckIn);

			btnRideLinqCheckIn.SetTitle(this.Services().Localize["HomeView_ManualPairing"], UIControlState.Normal);

            var set = this.CreateBindingSet<HomeView, HomeViewModel>();

            set.Bind(panelMenu)
                .For(v => v.DataContext)
                .To(vm => vm.Panel);

            set.Bind(btnMenu)
                .For(v => v.Command)
                .To(vm => vm.Panel.OpenOrCloseMenu);

			set.Bind(btnAirport)
				.For(v => v.Command)
				.To(vm => vm.AirportSearch);

            set.Bind(btnAirport)
                .For(v => v.Hidden)
                .To(vm => vm.Settings.IsAirportButtonEnabled)
                .WithConversion("BoolInverter");

			set.Bind(btnTrain)
				.For(v => v.Command)
                .To(vm => vm.TrainStationSearch);

			set.Bind(btnTrain)
				.For(v => v.Hidden)
				.To(vm => vm.Settings.IsTrainStationButtonEnabled)
				.WithConversion("BoolInverter");

            set.Bind(btnLocateMe)
                .For(v => v.Command)
                .To(vm => vm.LocateMe);

			set.Bind(btnRideLinqCheckIn)
				.For(v => v.Command)
				.To(vm => vm.ManualPairingRideLinq);

//            set.Bind(RideLinqSection)
//                .For(v => v.Hidden)
//                .To(vm => vm.IsManualRideLinqEnabled)
//                .WithConversion("BoolInverter");
//
//            set.Bind(btnRideLinqCheckIn)
//                .For(v => v.Hidden)
//                .To(vm => vm.IsManualRideLinqEnabled)
//                .WithConversion("BoolInverter");

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

            set.Apply();
        }

        public void ChangePresentation(ChangePresentationHint hint)
        {            
            if (hint is HomeViewModelPresentationHint)
            {
                ChangeState((HomeViewModelPresentationHint)hint);
            }
        }

        void ChangeState(HomeViewModelPresentationHint hint)
        {
			if (hint.State == HomeViewModelState.PickDate)
			{
				// Order Options: Visible
				// Order Review: Hidden
				// Order Edit: Hidden
				// Date Picker: Visible
				_datePicker.Show();
			}
			else if (hint.State == HomeViewModelState.Review)
			{
				// Order Options: Visible
				// Order Review: Visible
				// Order Edit: Hidden
				// Date Picker: Hidden
				UIView.Animate(
					0.6f, 
					() =>
					{
						orderEdit.SetNeedsDisplay();
						constraintOrderReviewTopSpace.Constant = 10;
						constraintOrderReviewBottomSpace.Constant = -65;
						constraintOrderOptionsTopSpace.Constant = 22;
						constraintOrderEditTrailingSpace.Constant = UIScreen.MainScreen.Bounds.Width;

						homeView.LayoutIfNeeded();  
						_datePicker.Hide();                                            
					}, () =>
					{
						RedrawSubViews();
					});
			}
			else if (hint.State == HomeViewModelState.Edit)
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
			else if (hint.State == HomeViewModelState.Initial)
			{
				// Order Options: Visible
				// Order Review: Hidden
				// Order Edit: Hidden
				// Date Picker: Hidden
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
				switch (hint.State)
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

        private void RedrawSubViews()
        {
            //redraw the shadows of the controls
            ctrlOrderReview.SetNeedsDisplay();
            orderEdit.SetNeedsDisplay();
            ctrlOrderOptions.SetNeedsDisplay();
        }
    }
}

