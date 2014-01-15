using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.Client.Navigation;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.BindingContext;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class ResetPasswordView : BaseViewController<ResetPasswordViewModel>, INavigationView
    {    
        public ResetPasswordView ()
            : base("ResetPasswordView", null)
        {
        }
        
		public bool HideNavigationBar
		{
			get
			{
				return true;
			}
		}

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();

			FlatButtonStyle.Main.ApplyTo(btnReset); 

			DismissKeyboardOnReturn(txtEmail);

			lblTitle.Text = Localize.GetValue ("LoginForgotPassword");
			lblSubTitle.Text = Localize.GetValue ("LoginForgotPasswordDetail");
			txtEmail.Placeholder = Localize.GetValue("CreateAccountEmail");
			btnReset.SetTitle(Localize.GetValue("ResetButton"), UIControlState.Normal);
			btnCancel.SetTitle(Localize.GetValue("CancelButton"), UIControlState.Normal);

            var set = this.CreateBindingSet<ResetPasswordView, ResetPasswordViewModel>();

            set.Bind(btnReset)
                .For("TouchUpInside")
                .To(x => x.ResetPassword);

            set.Bind(btnCancel)
                .For("TouchUpInside")
                .To(x => x.CloseCommand);

            set.Bind(txtEmail)
                .For(v => v.Text)
                .To(vm => vm.Email);

            set.Apply();
        }
    }
}

