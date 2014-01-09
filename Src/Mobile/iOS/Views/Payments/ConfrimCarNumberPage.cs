
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Views;
using Cirrious.MvvmCross.Binding.Touch.ExtensionMethods;
using System.Collections.Generic;
using apcurium.MK.Booking.Mobile.Client.Binding;

namespace apcurium.MK.Booking.Mobile.Client.Views.Payments
{
    public partial class ConfrimCarNumberPage :  BaseViewController<ConfirmCarNumberViewModel>
    {
        public ConfrimCarNumberPage(MvxShowViewModelRequest request) 
            : base(request)
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

            this.AddBindings(new Dictionary<object, string>{
                { ConfirmButton, new B("TouchDown","ConfirmTaxiNumber") }, 
                { CarNumber, new B("Text","CarNumber") }, 
            });
            
            View.ApplyAppFont ();
        }
    }
}

