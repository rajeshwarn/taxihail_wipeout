
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
    public partial class ResetPasswordView : BaseViewController<ResetPasswordViewModel>
    {
        #region Constructors
        
        public ResetPasswordView () 
            : base(new MvxShowViewModelRequest<ResetPasswordViewModel>( null, true, new Cirrious.MvvmCross.Interfaces.ViewModels.MvxRequestedBy()   ) )
        {
        }
        
        public ResetPasswordView(MvxShowViewModelRequest request) 
            : base(request)
        {
        }
        
        public ResetPasswordView(MvxShowViewModelRequest request, string nibName, NSBundle bundle) 
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

            base.DismissKeyboardOnReturn(txtEmail);

            var buttonsY = txtEmail.Frame.Y + txtEmail.Frame.Height + 25;
            AddButton(scrollView, 60, buttonsY, Resources.View_PasswordRecovery, "ResetPassword", apcurium.MK.Booking.Mobile.Client.AppStyle.ButtonColor.Green);

            this.AddBindings(new Dictionary<object, string>{
                { txtEmail, "{'Text': {'Path': 'Email', 'Mode': 'TwoWay' }}" },
            });

            this.NavigationItem.TitleView = new TitleView(null, Resources.GetValue("View_PasswordRecovery_Label"), true);


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

