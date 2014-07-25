using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.BindingContext;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;
using apcurium.MK.Booking.Mobile.Client.Style;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class AccountConfirmationView : BaseViewController<AccountConfirmationViewModel>
    {    
		public AccountConfirmationView () : base("AccountConfirmationView", null)
        {
        }
        
		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			NavigationController.NavigationBar.Hidden = true;
            NavigationController.NavigationBar.BarStyle = Theme.ShouldHaveLightContent(this.View.BackgroundColor)
                ? UIBarStyle.Black
                : UIBarStyle.Default;
		}

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
			txtCode.BecomeFirstResponder ();
		}

        public override void ViewDidLoad ()
        {
			base.ViewDidLoad ();

            View.BackgroundColor = Theme.LoginColor;
			lblTitle.TextColor = Theme.LabelTextColor;
			lblSubTitle.TextColor = Theme.LabelTextColor;
            lblTitle.TextColor = Theme.GetTextColor(Theme.LoginColor);
            lblSubTitle.TextColor = Theme.GetTextColor(Theme.LoginColor);

			FlatButtonStyle.Main.ApplyTo(btnConfirm); 
			btnConfirm.SetTitleColor(Theme.GetTextColor(Theme.LoginColor), UIControlState.Normal);

			DismissKeyboardOnReturn(txtCode);

			lblTitle.Text = Localize.GetValue ("View_AccountConfirmationTitle");
			lblSubTitle.Text = Localize.GetValue ("View_AccountConfirmation_Label_Instructions");
			txtCode.Placeholder = Localize.GetValue("View_AccountConfirmation_Label_Code");
			btnConfirm.SetTitle(Localize.GetValue("View_AccountConfirmation_Button"), UIControlState.Normal);

			var set = this.CreateBindingSet<AccountConfirmationView, AccountConfirmationViewModel>();

			set.Bind(btnConfirm)
                .For("TouchUpInside")
				.To(x => x.ConfirmAccount);           

			set.Bind(txtCode)
                .For(v => v.Text)
                .To(vm => vm.Code);

            set.Apply();
        }
    }
}

