using System;
using System.Linq;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.Framework.Extensions;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Views;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Client.Style;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class TermsAndConditionsView : BaseViewController<TermsAndConditionsViewModel>
    {
        public TermsAndConditionsView () 
			: base("TermsAndConditionsView", null)
        {
        }

        public override void ViewWillAppear (bool animated)
        {
            base.ViewWillAppear (animated);
            NavigationController.NavigationBar.Hidden = false;
			NavigationController.NavigationBar.BarStyle = Theme.IsLightContent ?
			                                              UIBarStyle.Black : UIBarStyle.Default;
        }

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad();

            View.BackgroundColor = UIColor.FromPatternImage(UIImage.FromFile("Assets/background.png"));
            NavigationItem.HidesBackButton = true;

            AppButtons.FormatStandardButton((GradientButton)btnAccept, Localize.GetValue("AcceptButton"), AppStyle.ButtonColor.Grey );
            AppButtons.FormatStandardButton((GradientButton)btnCancel, Localize.GetValue("Cancel"), AppStyle.ButtonColor.Grey);          

            new [] { 
                lblTitle
            }
            .Where(x => x != null)
                .ForEach(x => x.TextColor = AppStyle.DarkText)
                    .ForEach(x => x.Font = AppStyle.GetBoldFont(x.Font.PointSize));

            txtTermsAndConditions.TextColor = AppStyle.LightGreyText;
            txtTermsAndConditions.Font = AppStyle.GetNormalFont(txtTermsAndConditions.Font.PointSize);

            lblAcknowledgment.TextColor = AppStyle.GreyText;
            lblAcknowledgment.Font = AppStyle.GetNormalFont(txtTermsAndConditions.Font.PointSize);

            lblTitle.Text = Localize.GetValue("TermsAndConditionsLabel");
            lblAcknowledgment.Text = Localize.GetValue("TermsAndConditionsAcknowledgment");

			var set = this.CreateBindingSet<TermsAndConditionsView, TermsAndConditionsViewModel> ();

			set.BindSafe(btnAccept)
				.For("TouchUpInside")
				.To(vm => vm.AcceptTermsAndConditions);

			set.BindSafe(btnCancel)
				.For("TouchUpInside")
				.To(vm => vm.RejectTermsAndConditions);

			set.BindSafe(txtTermsAndConditions)
				.For(v => v.Text)
				.To(vm => vm.TermsAndConditions);

			set.Apply ();

            View.ApplyAppFont ();
        }

        public override void ViewDidAppear (bool animated)
        {
            base.ViewDidAppear (animated);
			NavigationItem.Title = string.Empty;
        }
    }
}
