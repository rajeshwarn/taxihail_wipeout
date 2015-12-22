using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.BindingContext;
using UIKit;
using apcurium.MK.Booking.Mobile.Client.Style;
using apcurium.MK.Common;

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

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            if (confirmScrollViewer.ContentSize.Width > UIScreen.MainScreen.Bounds.Width)
            {
                confirmScrollViewer.ContentSize = new CoreGraphics.CGSize(UIScreen.MainScreen.Bounds.Width, confirmScrollViewer.ContentSize.Height);
            }
        }

        public override void ViewDidLoad ()
        {
			base.ViewDidLoad ();

            View.BackgroundColor = Theme.LoginColor;
            confirmScrollViewer.BackgroundColor = Theme.LoginColor;

			lblTitle.TextColor = Theme.LabelTextColor;
			lblSubTitle.TextColor = Theme.LabelTextColor;
            lblTitle.TextColor = Theme.GetContrastBasedColor(Theme.LoginColor);
            lblSubTitle.TextColor = Theme.GetContrastBasedColor(Theme.LoginColor);

			btnConfirm.SetTitleColor(Theme.GetContrastBasedColor(Theme.LoginColor), UIControlState.Normal);
            btnConfirm.SetStrokeColor(Theme.GetContrastBasedColor(Theme.LoginColor));

            btnResend.SetTitleColor(Theme.GetContrastBasedColor(Theme.LoginColor), UIControlState.Normal);
            btnResend.SetStrokeColor(Theme.GetContrastBasedColor(Theme.LoginColor));

			DismissKeyboardOnReturn(txtCode);

			lblTitle.Text = Localize.GetValue ("View_AccountConfirmationTitle");
			lblSubTitle.Text = Localize.GetValue ("View_AccountConfirmation_Label_Instructions");
			txtCode.Placeholder = Localize.GetValue("View_AccountConfirmation_Label_Code");
            txtCode.AccessibilityLabel = txtCode.Placeholder;
			btnConfirm.SetTitle(Localize.GetValue("View_AccountConfirmation_Button"), UIControlState.Normal);
            btnResend.SetTitle(Localize.GetValue("ResendConfirmationCodeButtonText"), UIControlState.Normal);
            lblPhoneNumberTitle.Text = Localize.GetValue("RideSettingsPhone");
            txtPhoneNumber.Placeholder = Localize.GetValue("RideSettingsPhone");
            txtPhoneNumber.AccessibilityLabel = txtPhoneNumber.Placeholder;
            lblDialCode.AccessibilityLabel = Localize.GetValue("DialCodeSelectorTitle");
            lblDialCode.TintColor = UIColor.FromRGB (44, 44, 44); // cursor color
            lblDialCode.TextColor = UIColor.FromRGB(44, 44, 44);
            lblDialCode.Font = UIFont.FromName(FontName.HelveticaNeueLight, 38/2);
            lblDialCode.Configure(this.NavigationController, (DataContext as AccountConfirmationViewModel).PhoneNumber);
            lblDialCode.NotifyChanges += (sender, e) => ViewModel.SelectedCountryCode = CountryCode.GetCountryCodeByIndex(CountryCode.GetCountryCodeIndexByCountryISOCode(e.Country));

			var set = this.CreateBindingSet<AccountConfirmationView, AccountConfirmationViewModel>();

			set.Bind(btnConfirm)
                .For("TouchUpInside")
				.To(x => x.ConfirmAccount);           

			set.Bind(txtCode)
                .For(v => v.Text)
                .To(vm => vm.Code);

            set.Bind(btnResend)
                .For("TouchUpInside")
                .To(x => x.ResendConfirmationCode);

            set.Bind(txtPhoneNumber)
                .For(v => v.Text)
                .To(vm => vm.Phone);

            set.Apply();
        }
    }
}