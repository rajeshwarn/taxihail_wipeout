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


        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();

			base.DismissKeyboardOnReturn(txtEmail);

			/*Add border on button*/
			btnCancel.Layer.BorderWidth = 0.5f;
			btnCancel.Layer.BorderColor = UIColor.Black.CGColor;
			btnCancel.Layer.CornerRadius = 0.5f;

			lblTitle.Text = Localize.GetValue ("LoginForgotPassword");
			lblSubTitle.Text = Localize.GetValue ("LoginForgotPasswordDetail");
			txtEmail.Placeholder = Localize.GetValue("CreateAccountEmail");
			btnReset.SetTitle(Localize.GetValue("ResetButton"), UIControlState.Normal);
			btnCancel.SetTitle(Localize.GetValue("CancelButton"), UIControlState.Normal);

			this.AddBindings(new Dictionary<object, string>{
                { txtEmail, "{'Text': {'Path': 'Email', 'Mode': 'TwoWay' }}" },
				{ btnReset, "{'TouchUpInside': {'Path' : 'ResetPassword'}}" },
				{ btnCancel, "{'TouchUpInside': {'Path' : 'Cancel'}}" }
            });
        }
    }
}

