using System.Collections.Generic;
using System.Drawing;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Views;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Binding.BindingContext;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class ResetPasswordView : BaseViewController<ResetPasswordViewModel>
    {
        public ResetPasswordView() 
			: base("ResetPasswordView", null)
        {
        }
        
        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();
			
            scrollView.ContentSize = new SizeF(scrollView.ContentSize.Width, 416);

            lblEmail.Text = Localize.GetValue("CreateAccountEmail");

            DismissKeyboardOnReturn(txtEmail);

            var buttonsY = txtEmail.Frame.Y + txtEmail.Frame.Height + 25;
            AddButton(scrollView, 60, buttonsY, Localize.GetValue("View_PasswordRecovery"), "ResetPassword", AppStyle.ButtonColor.Green);

			var set = this.CreateBindingSet<ResetPasswordView, ResetPasswordViewModel> ();

			set.Bind(txtEmail)
				.For(v => v.Text)
				.To(vm => vm.Email);

			set.Apply ();

            NavigationItem.TitleView = new TitleView(null, Localize.GetValue("View_PasswordRecovery_Label"), true);

            View.ApplyAppFont ();

        }
        private void AddButton(UIView parent, float x, float y, string title, string command, AppStyle.ButtonColor bcolor)
        {
            var btn = AppButtons.CreateStandardButton(new RectangleF(x, y, 200, 40), title, bcolor);
            btn.TextShadowColor = null;
            parent.AddSubview(btn);

			var set = this.CreateBindingSet<ResetPasswordView, ResetPasswordViewModel> ();

			set.Bind(btn)
				.For("TouchUpInside")
				.To(command);    

			set.Apply ();
        }

    }
}

