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
    public partial class CreateAccountView : BaseViewController
    {
        public CreateAccountView() 
			: base("CreateAccountView", null)
        {
        }
		
        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();
			
            scrollView.ContentSize = new SizeF(scrollView.ContentSize.Width, 416);

            lblEmail.Text = Resources.GetValue("CreateAccountEmail");
            lblName.Text = Resources.GetValue("CreateAccountFullName");
            lblPhone.Text = Resources.GetValue("CreateAccountPhone");
            lblPassword.Text = Resources.GetValue("CreateAccountPassword");
            lblConfirmPassword.Text = Resources.GetValue("CreateAccountPasswordConfrimation");

            DismissKeyboardOnReturn(txtEmail, txtName, txtPhone, txtPassword, txtConfirmPassword);
            
            txtPassword.SecureTextEntry = true;
            txtConfirmPassword.SecureTextEntry = true;

            var buttonsY = txtConfirmPassword.Frame.Y + txtConfirmPassword.Frame.Height + 25;
            AddButton(scrollView, 60, buttonsY, Resources.CreateAccountCreate, "CreateAccount", AppStyle.ButtonColor.Green);

            this.AddBindings(new Dictionary<object, string>{
                { txtName, "{'Text': {'Path': 'Data.Name', 'Mode': 'TwoWay' }}" },
                { txtEmail, "{'Text': {'Path': 'Data.Email', 'Mode': 'TwoWay' }}" },
                { txtPhone, "{'Text': {'Path': 'Data.Phone', 'Mode': 'TwoWay' }}" },
                { txtPassword, "{'Text': {'Path': 'Data.Password', 'Mode': 'TwoWay' },'Hidden': {'Path': 'HasSocialInfo'}}" },
                { txtConfirmPassword, "{'Text': {'Path': 'ConfirmPassword', 'Mode': 'TwoWay' },'Hidden': {'Path': 'HasSocialInfo'}}" },
                { lblPassword, "{'Hidden': {'Path': 'HasSocialInfo'}}" },
                { lblConfirmPassword, "{'Hidden': {'Path': 'HasSocialInfo'}}" },
            });

            NavigationItem.TitleView = new TitleView(null, Resources.GetValue("View_SignUp"), true);


            View.ApplyAppFont ();

        }
		
       
        private void AddButton(UIView parent, float x, float y, string title, string command, AppStyle.ButtonColor bcolor)
        {
            var btn = AppButtons.CreateStandardButton(new RectangleF(x, y, 200, 40), title, bcolor);
            btn.TextShadowColor = null;
            parent.AddSubview(btn);
            this.AddBindings(btn, "{'TouchUpInside': {'Path' : '" + command + "'}}");              
        }

    }
}

