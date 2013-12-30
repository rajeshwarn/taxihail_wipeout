
using System;
using System.Drawing;
using apcurium.MK.Booking.Mobile.ViewModels;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Binding.Touch.Views;
using Cirrious.MvvmCross.Views;
using System.Collections.Generic;
using Cirrious.MvvmCross.Binding.Touch.ExtensionMethods;

namespace apcurium.MK.Booking.Mobile.Client
{
    public partial class CreateAccountView : BaseViewController<CreateAcccountViewModel>
    {
        #region Constructors
        
        public CreateAccountView () 
            : base(new MvxShowViewModelRequest<CreateAcccountViewModel>( null, true, new Cirrious.MvvmCross.Interfaces.ViewModels.MvxRequestedBy()   ) )
        {
        }
        
        public CreateAccountView(MvxShowViewModelRequest request) 
            : base(request)
        {
        }
        
        public CreateAccountView(MvxShowViewModelRequest request, string nibName, NSBundle bundle) 
            : base(request, nibName, bundle)
        {
        }
        
#endregion
		
        public override void DidReceiveMemoryWarning ()
        {
            // Releases the view if it doesn't have a superview.
            base.DidReceiveMemoryWarning ();
			
            // Release any cached data, images, etc that aren't in use.
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

            base.DismissKeyboardOnReturn(txtEmail, txtName, txtPhone, txtPassword, txtConfirmPassword);
            
            txtPassword.SecureTextEntry = true;
            txtConfirmPassword.SecureTextEntry = true;

            var buttonsY = txtConfirmPassword.Frame.Y + txtConfirmPassword.Frame.Height + 25;
            AddButton(scrollView, 60, buttonsY, Resources.CreateAccountCreate, "CreateAccount", apcurium.MK.Booking.Mobile.Client.AppStyle.ButtonColor.Green);

            this.AddBindings(new Dictionary<object, string>{
                { txtName, "{'Text': {'Path': 'Data.Name', 'Mode': 'TwoWay' }}" },
                { txtEmail, "{'Text': {'Path': 'Data.Email', 'Mode': 'TwoWay' }}" },
                { txtPhone, "{'Text': {'Path': 'Data.Phone', 'Mode': 'TwoWay' }}" },
                { txtPassword, "{'Text': {'Path': 'Data.Password', 'Mode': 'TwoWay' },'Hidden': {'Path': 'HasSocialInfo'}}" },
                { txtConfirmPassword, "{'Text': {'Path': 'ConfirmPassword', 'Mode': 'TwoWay' },'Hidden': {'Path': 'HasSocialInfo'}}" },
                { lblPassword, "{'Hidden': {'Path': 'HasSocialInfo'}}" },
                { lblConfirmPassword, "{'Hidden': {'Path': 'HasSocialInfo'}}" },
            });

            this.NavigationItem.TitleView = new TitleView(null, Resources.GetValue("View_SignUp"), true);


            this.View.ApplyAppFont ();

        }
		
       
        private void AddButton(UIView parent, float x, float y, string title, string command, AppStyle.ButtonColor bcolor)
        {
            var btn = AppButtons.CreateStandardButton(new System.Drawing.RectangleF(x, y, 200, 40), title, bcolor);
            btn.TextShadowColor = null;
            parent.AddSubview(btn);
            this.AddBindings(btn, "{'TouchUpInside': {'Path' : '" + command + "'}}");              
        }

    }
}

