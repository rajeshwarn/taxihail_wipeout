using System.Collections.Generic;
using System.Drawing;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Views;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class CreateAccountView : BaseViewController<CreateAccountViewModel>
    {
        public CreateAccountView() 
			: base("CreateAccountView", null)
        {
        }

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			NavigationController.NavigationBar.Hidden = false;
		}

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();

            scrollView.ContentSize = new SizeF(scrollView.ContentSize.Width, 416);

            lblEmail.Text = Localize.GetValue("CreateAccountEmail");
            lblName.Text = Localize.GetValue("CreateAccountFullName");
            lblPhone.Text = Localize.GetValue("CreateAccountPhone");
            lblPassword.Text = Localize.GetValue("CreateAccountPassword");
            lblConfirmPassword.Text = Localize.GetValue("CreateAccountPasswordConfrimation");

            DismissKeyboardOnReturn(txtEmail, txtName, txtPhone, txtPassword, txtConfirmPassword);
            
            txtPassword.SecureTextEntry = true;
            txtConfirmPassword.SecureTextEntry = true;

			var buttonsY = txtConfirmPassword.Frame.Y + txtConfirmPassword.Frame.Height + 25;
			AddButton(scrollView, 60, buttonsY, Localize.GetValue("CreateAccountCreate"), "CreateAccount", AppStyle.ButtonColor.Green);

			var set = this.CreateBindingSet<CreateAccountView, CreateAccountViewModel>();

			set.Bind(txtName)
				.For(v => v.Text)
				.To(vm => vm.Data.Name);

			set.Bind(txtEmail)
				.For(v => v.Text)
				.To(vm => vm.Data.Email);

			set.Bind(txtPhone)
				.For(v => v.Text)
				.To(vm => vm.Data.Phone);

			set.Bind(txtPassword)
				.For(v => v.Text)
				.To(vm => vm.Data.Password);
			set.Bind(txtPassword)
				.For(v => v.Hidden)
				.To(vm => vm.HasSocialInfo);

			set.Bind(txtConfirmPassword)
				.For(v => v.Text)
				.To(vm => vm.ConfirmPassword);
			set.Bind(txtConfirmPassword)
				.For(v => v.Hidden)
				.To(vm => vm.HasSocialInfo);

			set.Bind(lblPassword)
				.For(v => v.Hidden)
				.To(vm => vm.HasSocialInfo);

			set.Bind(lblConfirmPassword)
				.For(v => v.Hidden)
				.To(vm => vm.HasSocialInfo);

			set.Apply ();

			NavigationItem.Title = Localize.GetValue("View_SignUp");

            View.ApplyAppFont ();
        }

        private void AddButton(UIView parent, float x, float y, string title, string command, AppStyle.ButtonColor bcolor)
        {
            var btn = AppButtons.CreateStandardButton(new RectangleF(x, y, 200, 40), title, bcolor);
            btn.TextShadowColor = null;
            parent.AddSubview(btn);

			var set = this.CreateBindingSet<CreateAccountView, CreateAccountViewModel>();

			set.Bind(btn)
				.For("TouchUpInside")
				.To(command);

			set.Apply ();            
        }
    }
}

