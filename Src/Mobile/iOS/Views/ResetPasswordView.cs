using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.BindingContext;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;
using apcurium.MK.Booking.Mobile.Client.Style;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class ResetPasswordView : BaseViewController<ResetPasswordViewModel>
    {    
		public ResetPasswordView () : base("ResetPasswordView", null)
        {
        }
        
		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			NavigationController.NavigationBar.Hidden = true;
		}

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();

			View.BackgroundColor = Theme.BackgroundColor;
			lblTitle.TextColor = Theme.LabelTextColor;
			lblSubTitle.TextColor = Theme.LabelTextColor;

			FlatButtonStyle.Main.ApplyTo(btnReset); 

			DismissKeyboardOnReturn(txtEmail);

			lblTitle.Text = Localize.GetValue ("ResetPasswordTitleText");
			lblSubTitle.Text = Localize.GetValue ("ResetPasswordSubtitleText");
			txtEmail.Placeholder = Localize.GetValue("ResetPasswordEmailPlaceHolder");
			btnReset.SetTitle(Localize.GetValue("Reset"), UIControlState.Normal);
			btnCancel.SetTitle(Localize.GetValue("Cancel"), UIControlState.Normal);

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

