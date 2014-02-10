using System;
using System.Drawing;
using Cirrious.MvvmCross.Binding.BindingContext;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets.Booking;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.Client.MapUtitilties;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using apcurium.MK.Booking.Mobile.PresentationHints;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
    public partial class HomeView : BaseViewController<HomeViewModel>, IChangePresentation
    {
        private bool _defaultThemeApplied;
        private PanelMenuView _menu;
        private BookLaterDatePicker _datePicker;

        public HomeView() : base("HomeView", null)
        {
        }

        public override void ViewWillAppear (bool animated)
        {
            base.ViewWillAppear (animated);

            if (!_defaultThemeApplied)
            {
                // reset to default theme for the navigation bar
                ChangeThemeOfNavigationBar();
                _defaultThemeApplied = true;
            }
            NavigationController.NavigationBar.BarStyle = UIBarStyle.Default;
            NavigationController.NavigationBar.Hidden = true;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            ChangeThemeOfNavigationBar();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            btnMenu.SetImage(UIImage.FromFile("menu_icon.png"), UIControlState.Normal);
            btnMenu.SetImage(UIImage.FromFile("menu_icon_pressed.png"), UIControlState.Highlighted);

            btnLocateMe.SetImage(UIImage.FromFile("location_icon.png"), UIControlState.Normal);
            btnLocateMe.SetImage(UIImage.FromFile("location_icon_pressed.png"), UIControlState.Highlighted);

            mapView.Delegate = new AddressMapDelegate (true);

            InstantiatePanel();

            _datePicker = new BookLaterDatePicker();            
            _datePicker.UpdateView(View.Frame.Height, View.Frame.Width);
            _datePicker.Hide();
            View.AddSubview(_datePicker);

            var set = this.CreateBindingSet<HomeView, HomeViewModel>();

            set.Bind(_menu)
                .For(v => v.DataContext)
                .To(vm => vm.Panel);

            set.Bind(btnMenu)
                .For(v => v.Command)
                .To(vm => vm.Panel.OpenOrCloseMenu);

            set.Bind(btnLocateMe)
                .For(v => v.Command)
                .To(vm => vm.LocateMe);

            set.Bind(mapView)
                .For(v => v.DataContext)
                .To(vm => vm.Map);

            set.Bind(ctrlOrderOptions)
                .For(v => v.DataContext)
                .To(vm => vm.OrderOptions);

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

        public void ChangeState(ChangeStatePresentationHint hint)
        {
            ChangeState((HomeViewModelPresentationHint)hint);
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
                        constraintOrderReviewTopSpace.Constant = 10;
                        constraintOrderReviewVerticalSpace.Constant = -50;
                        constraintOrderOptionsTopSpace.Constant =  22;
                        constraintOrderEditTrailingSpace.Constant = UIScreen.MainScreen.Bounds.Width;
                        homeView.LayoutIfNeeded();
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
                    () => {
                        constraintOrderReviewTopSpace.Constant = UIScreen.MainScreen.Bounds.Height;
                        constraintOrderReviewVerticalSpace.Constant = 537;
                        constraintOrderOptionsTopSpace.Constant =  - ctrlOrderOptions.Frame.Height;
                        constraintOrderEditTrailingSpace.Constant = 8;
                        homeView.LayoutIfNeeded();
                    });
            }
            else if(hint.State == HomeViewModelState.Initial)
            {
                // Order Options: Visible
                // Order Review: Hidden
                // Order Edit: Hidden
                // Date Picker: Hidden
                UIView.Animate(
                    0.6f, 
                    () => {
                        constraintOrderReviewTopSpace.Constant = UIScreen.MainScreen.Bounds.Height;
                        constraintOrderReviewVerticalSpace.Constant = 537;
                        constraintOrderOptionsTopSpace.Constant =  22;
                        constraintOrderEditTrailingSpace.Constant = UIScreen.MainScreen.Bounds.Width;
                        homeView.LayoutIfNeeded();
                    });
            }
           
            ctrlOrderOptions.ChangeState(hint);
            bottomBar.ChangeState(hint);
        }

        private void InstantiatePanel()
        {
            var nib = UINib.FromName ("PanelMenuView", null);
            _menu = (PanelMenuView)nib.Instantiate (this, null)[0];
            _menu.ViewToAnimate = homeView;

            View.InsertSubviewBelow (_menu, homeView);
        }
    }
}

