using System.Collections.Generic;
using System.Drawing;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.Client.Navigation;
using apcurium.MK.Booking.Mobile.Client.Views;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.Touch.ExtensionMethods;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using Cirrious.MvvmCross.Views;
using MonoTouch.Foundation;
using MonoTouch.UIKit;


namespace apcurium.MK.Booking.Mobile.Client.Views
{
    public partial class ResetPasswordView : BaseViewController<ResetPasswordViewModel>, INavigationView
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

        #region INavigationView implementation

        public bool HideNavigationBar
        {
            get
            {
                return true;
            }
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

			var logo = UIImage.FromFile("Assets/Logo.png");
			imageViewLogo.Image = logo;

			scrollView.ContentSize = new SizeF(scrollView.ContentSize.Width, 416);

			base.DismissKeyboardOnReturn(txtEmail);

			txtEmail.Placeholder = Localize.GetValue("CreateAccountEmail");

			this.AddBindings(new Dictionary<object, string>{
                { txtEmail, "{'Text': {'Path': 'Email', 'Mode': 'TwoWay' }}" },
            });
             
			btnReset.SetTitle(Localize.GetValue("View_PasswordRecovery_Label"), UIControlState.Normal);
			btnCancel.SetTitle(Localize.GetValue("CancelButton"), UIControlState.Normal);

			this.AddBindings(btnReset, "{'TouchUpInside': {'Path' : 'ResetPassword'}}"); 
			this.AddBindings(btnCancel, "{'TouchUpInside': {'Path' : 'Cancel'}}"); 

        }
    }
}

