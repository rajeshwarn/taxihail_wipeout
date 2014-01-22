
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
    [MvxViewFor(typeof(ConfirmPaymentViewModel))]
    public partial class ConfirmPaymentPage :  BaseViewController<ConfirmPaymentViewModel>
    {
        public ConfirmPaymentPage() 
            : base("ConfirmPaymentPage", null)
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

            var set = this.CreateBindingSet<ConfirmPaymentPage, ConfirmPaymentViewModel>();

			set.Bind(ConfirmButton)
				.For("TouchDown")
                .To(vm => vm.ConfirmPayment);

			set.Bind(CarNumber)
				.For(v => v.Text)
				.To(vm => vm.CarNumber);

			set.Apply ();

            View.ApplyAppFont ();
        }
    }
}

