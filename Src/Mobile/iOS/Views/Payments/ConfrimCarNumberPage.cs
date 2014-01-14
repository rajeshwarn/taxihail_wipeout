
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Views;
using System.Collections.Generic;
using apcurium.MK.Booking.Mobile.Client.Binding;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Binding.BindingContext;

namespace apcurium.MK.Booking.Mobile.Client.Views.Payments
{
	[MvxViewFor(typeof(ConfirmCarNumberViewModel))]
	public partial class ConfrimCarNumberPage :  BaseViewController<ConfirmCarNumberViewModel>
    {
        public ConfrimCarNumberPage() 
			: base("ConfrimCarNumberPage", null)
        {
        }

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();

            Container.BackgroundColor = UIColor.FromPatternImage (UIImage.FromFile ("Assets/background.png"));

            lblConfirmDriverInfo.Text = Localize.GetValue("VehicleNumberInfo");
            lblConfirmDriverNotice.Text = Localize.GetValue("VehicleNumberNotice");
            lblConfirmDriverNotice.Font = AppStyle.GetNormalFont (13);

            AppButtons.FormatStandardButton((GradientButton)ConfirmButton, Localize.GetValue("ConfirmButton"), AppStyle.ButtonColor.Green); 

			var set = this.CreateBindingSet<ConfrimCarNumberPage, ConfirmCarNumberViewModel>();
			set.Bind(ConfirmButton)
				.For("TouchDown")
				.To(vm => vm.ConfirmTaxiNumber);

			set.Bind(CarNumber)
				.For(v => v.Text)
				.To(vm => vm.CarNumber);

			set.Apply ();

            View.ApplyAppFont ();
        }
    }
}

