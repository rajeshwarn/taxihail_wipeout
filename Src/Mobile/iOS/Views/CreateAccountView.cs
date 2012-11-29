
using System;
using System.Drawing;

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

            lblEmail.Text = Resources.GetValue("CreateAccoutEmailLabel");
            lblName.Text = Resources.GetValue("CreateAccountFullName");
            lblPhone.Text = Resources.GetValue("CreateAccoutPhoneNumberLabel");
            lblPassword.Text = Resources.GetValue("CreateAccoutPasswordLabel");
            lblConfirmPassword.Text = Resources.GetValue("CreateAccountPasswordConfrimation");

            txtEmail.ReturnKeyType = UIReturnKeyType.Done;
            txtName.ReturnKeyType = UIReturnKeyType.Done;
            txtPhone.ReturnKeyType = UIReturnKeyType.Done;
            txtPassword.SecureTextEntry = true;
            txtPassword.ReturnKeyType = UIReturnKeyType.Done;
            txtConfirmPassword.SecureTextEntry = true;
            txtConfirmPassword.ReturnKeyType = UIReturnKeyType.Done;

            var buttonsY = txtConfirmPassword.Frame.Y + txtConfirmPassword.Frame.Height + 25;
            AddButton(scrollView, 95, buttonsY, Resources.CreateAccoutCreate, "CreateAccount", apcurium.MK.Booking.Mobile.Client.AppStyle.ButtonColor.Green);

            this.AddBindings(new Dictionary<object, string>{
                { txtName, "{'Text': {'Path': 'Data.Name', 'Mode': 'TwoWay' }}" },
                { txtEmail, "{'Text': {'Path': 'Data.Email', 'Mode': 'TwoWay' }}" },
                { txtPhone, "{'Text': {'Path': 'Data.Phone', 'Mode': 'TwoWay' }}" },
                { txtPassword, "{'Text': {'Path': 'Data.Password', 'Mode': 'TwoWay' }}" },
                { txtConfirmPassword, "{'Text': {'Path': 'ConfirmPassword', 'Mode': 'TwoWay' }}" },
            });

        }
		
        public override void ViewDidUnload ()
        {
            base.ViewDidUnload ();
			
            // Clear any references to subviews of the main view in order to
            // allow the Garbage Collector to collect them sooner.
            //
            // e.g. myOutlet.Dispose (); myOutlet = null;
			
            ReleaseDesignerOutlets ();
        }
		
        public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
        {
            // Return true for supported orientations
            return (toInterfaceOrientation != UIInterfaceOrientation.PortraitUpsideDown);
        }

        private void AddButton(UIView parent, float x, float y, string title, string command, AppStyle.ButtonColor bcolor)
        {
            var btn = AppButtons.CreateStandardButton(new System.Drawing.RectangleF(x, y, 130, 40), title, bcolor);
            btn.TextShadowColor = null;
            parent.AddSubview(btn);
            this.AddBindings(btn, "{'TouchUpInside': {'Path' : '" + command + "'}}");              
        }

    }
}

