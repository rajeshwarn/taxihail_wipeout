using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.Client.MapUtitilties;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Data;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
    public partial class HomeView : BaseViewController<HomeViewModel>
    {
        private bool _defaultThemeApplied;
        private PanelMenuView _menu;

        public HomeView() : base("HomeView", null)
        {
        }

        public override void ViewWillAppear (bool animated)
        {
            base.ViewWillAppear (animated);

            if (!_defaultThemeApplied)
            {
                // reset to default theme for the navigation bar
                ChangeThemeOfNavigationBar(true);
                _defaultThemeApplied = true;
            }

            NavigationController.NavigationBar.Hidden = true;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            btnMenu.SetImage(UIImage.FromFile("menu_icon.png"), UIControlState.Normal);
            btnMenu.SetImage(UIImage.FromFile("menu_icon_pressed.png"), UIControlState.Highlighted);

            btnLocateMe.SetImage(UIImage.FromFile("location_icon.png"), UIControlState.Normal);
            btnLocateMe.SetImage(UIImage.FromFile("location_icon_pressed.png"), UIControlState.Highlighted);

            // temporary for testing purpose
            var btnEstimate = new UIButton(new RectangleF(4, 9, 64, 42));
            btnEstimate.BackgroundColor = UIColor.Blue;
            btnEstimate.SetTitle("$", UIControlState.Normal);
            btnEstimate.TouchUpInside += (sender, e) => ViewModel.AddressSelectionMode = ViewModel.AddressSelectionMode == AddressSelectionMode.DropoffSelection 
                ? AddressSelectionMode.PickupSelection
                : AddressSelectionMode.DropoffSelection;
            bottomBar.AddSubview(btnEstimate);

            InstantiatePanel();

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
                .To(vm => vm);
                
            set.Apply();
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

