using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.Client.MapUtitilties;
using Cirrious.MvvmCross.Binding.BindingContext;

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

            var btn = new UIButton(new RectangleF(0, 0, 21, 21));
            btn.SetImage(UIImage.FromFile("Assets/settings.png"), UIControlState.Normal);
            btn.TouchUpInside += (sender, e) => ViewModel.Panel.MenuIsOpen = !ViewModel.Panel.MenuIsOpen;
            test.AddSubview(btn);

            var btn2 = new UIButton(new RectangleF(0, 0, 21, 21));
            btn2.SetImage(UIImage.FromFile("Assets/gpsRefreshIcon.png"), UIControlState.Normal);
            locateMeOverlay.AddSubview(btn2);

            InstantiatePanel();

            var set = this.CreateBindingSet<HomeView, HomeViewModel>();

            set.Bind(_menu)
                .For(v => v.DataContext)
                .To(vm => vm.Panel);

            set.Bind(btn2)
                .For("TouchUpInside")
                .To(vm => vm.LocateMe);

            set.Bind(txtPickupAddress)
                .To(vm => vm.PickupAddress.DisplayAddress);

            set.Bind(mapView)
                .For(v => v.DataContext)
                .To(vm => vm.Map);

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

