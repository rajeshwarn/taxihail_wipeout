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
    public partial class ResetPasswordView : BaseViewController
    {
        public ResetPasswordView() 
			: base("ResetPasswordView", null)
        {
        }
        
        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();
			
            scrollView.ContentSize = new SizeF(scrollView.ContentSize.Width, 416);

            lblEmail.Text = Resources.GetValue("CreateAccountEmail");

            DismissKeyboardOnReturn(txtEmail);

            var buttonsY = txtEmail.Frame.Y + txtEmail.Frame.Height + 25;
            AddButton(scrollView, 60, buttonsY, Resources.View_PasswordRecovery, "ResetPassword", AppStyle.ButtonColor.Green);

            this.AddBindings(new Dictionary<object, string>{
                { txtEmail, "{'Text': {'Path': 'Email', 'Mode': 'TwoWay' }}" },
            });

            NavigationItem.TitleView = new TitleView(null, Resources.GetValue("View_PasswordRecovery_Label"), true);

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

