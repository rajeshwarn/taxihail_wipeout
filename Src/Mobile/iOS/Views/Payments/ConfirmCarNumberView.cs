
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Views;
using System.Collections.Generic;
using apcurium.MK.Booking.Mobile.Client.Binding;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;

namespace apcurium.MK.Booking.Mobile.Client.Views.Payments
{
    public partial class ConfirmCarNumberView :  BaseViewController<ConfirmCarNumberViewModel>
    {
        public ConfirmCarNumberView() : base("ConfirmCarNumberView", null)
        {
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            NavigationController.NavigationBar.Hidden = false;
            NavigationItem.Title = Localize.GetValue("View_ConfirmCarNumber");
            NavigationItem.HidesBackButton = false;
        }

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();

            View.BackgroundColor = UIColor.FromRGB(242, 242, 242);

            FlatButtonStyle.Green.ApplyTo(btnConfirm);

            lblConfirmDriverInfo.Text = Localize.GetValue("VehicleNumberInfo");
            lblConfirmDriverNotice.Text = Localize.GetValue("VehicleNumberNotice");
            btnConfirm.SetTitle(Localize.GetValue("Confirm"), UIControlState.Normal);

            var set = this.CreateBindingSet<ConfirmCarNumberView, ConfirmCarNumberViewModel>();

            set.Bind(btnConfirm)
                .For("TouchUpInside")
                .To(vm => vm.ConfirmCarNumber);

            set.Bind(lblCarNumber)
				.For(v => v.Text)
				.To(vm => vm.CarNumber);

			set.Apply ();
        }
    }
}

