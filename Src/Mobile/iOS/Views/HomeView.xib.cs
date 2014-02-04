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

namespace apcurium.MK.Booking.Mobile.Client.Views
{
    public partial class HomeView : BaseViewController<HomeViewModel>
    {
        private bool _defaultThemeApplied;
        private PanelMenuView _menu;
        private BookLaterDatePicker _bookLaterDatePicker;

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

            AddButtonsToAppBar();

            InstantiatePanel();
            AddBookLaterDatePicker();

            var set = this.CreateBindingSet<HomeView, HomeViewModel>();

            set.Bind(_menu)
                .For(v => v.DataContext)
                .To(vm => vm.Panel);

            set.Bind(_bookLaterDatePicker)
                .For(v => v.DataContext)
                .To(vm => vm.BookLater);

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
                
            set.Apply();
        }

        private void AddButtonsToAppBar()
        {
            // temporary way to add these buttons
            var btnEstimate = new AppBarButton(Localize.GetValue("Estimate"), AppBarView.ButtonSize.Width, AppBarView.ButtonSize.Height, "estimate_icon.png", "estimate_icon_pressed.png");
            btnEstimate.Frame = btnEstimate.Frame.IncrementX(4);

            var btnBook = new FlatButton(new RectangleF(99, 7, 123, 41));
            FlatButtonStyle.Green.ApplyTo(btnBook);
            btnBook.SetTitle(Localize.GetValue("BookItButton"), UIControlState.Normal);

            var btnBookLater = new AppBarButton(Localize.GetValue("BookItLaterButton"), AppBarView.ButtonSize.Width, AppBarView.ButtonSize.Height, "later_icon.png", "later_icon_pressed.png");
            btnBookLater.Frame = btnBookLater.Frame.SetX(View.Frame.Width - btnBookLater.Frame.Width - 3);
            // test to show how the order options control will show up in the confirmation screen
            btnBookLater.TouchUpInside += (sender, e) => _bookLaterDatePicker.Show();

            var set = this.CreateBindingSet<HomeView, HomeViewModel>();

            set.Bind(btnEstimate)
                .For(v => v.Command)
                .To(vm => vm.ChangeAddressSelectionMode);
            set.Bind(btnEstimate)
                .For(v => v.Selected)
                .To(vm => vm.OrderOptions.ShowDestination);

            set.Apply();

            bottomBar.AddSubviews(btnEstimate, btnBook, btnBookLater);
        }

        private void InstantiatePanel()
        {
            var nib = UINib.FromName ("PanelMenuView", null);
            _menu = (PanelMenuView)nib.Instantiate (this, null)[0];
            _menu.ViewToAnimate = homeView;

            View.InsertSubviewBelow (_menu, homeView);
        }

        private void AddBookLaterDatePicker()
        {
            _bookLaterDatePicker = new BookLaterDatePicker();            
            _bookLaterDatePicker.UpdateView(View.Frame.Height, View.Frame.Width);
            _bookLaterDatePicker.Hide();
            View.AddSubview(_bookLaterDatePicker);
        }
    }
}

