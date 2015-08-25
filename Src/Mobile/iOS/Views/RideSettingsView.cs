using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.ViewModels;
using UIKit;
using Cirrious.MvvmCross.Binding.BindingContext;
using System.Windows.Input;
using apcurium.MK.Common;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class RideSettingsView : BaseViewController<RideSettingsViewModel>
    {              
        public RideSettingsView() 
			: base("RideSettingsView", null)
        {
        }
		
        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            NavigationController.NavigationBar.Hidden = false;
            NavigationItem.Title = Localize.GetValue("View_RideSettings");

            ChangeThemeOfBarStyle();
            ChangeRightBarButtonFontToBold();
        }

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();

            View.BackgroundColor = UIColor.FromRGB(242, 242, 242);

			if (!ViewModel.IsChargeAccountPaymentEnabled)
			{
				lblAccountNumber.RemoveFromSuperview();
                lblCustomerNumber.RemoveFromSuperview();
				txtAccountNumber.RemoveFromSuperview();
                txtCustomerNumber.RemoveFromSuperview();
			}

			if (!ViewModel.ShouldDisplayTip)
            {
                lblTip.RemoveFromSuperview();
                txtTip.RemoveFromSuperview();
			}

			if (!ViewModel.IsPayBackFieldEnabled)
			{
				lblPayBack.RemoveFromSuperview();
				txtPayBack.RemoveFromSuperview();
			}

            if (!ViewModel.IsVehicleTypeSelectionEnabled)
            {
                lblVehicleType.RemoveFromSuperview();
                txtVehicleType.RemoveFromSuperview();
            }

            lblName.Text = Localize.GetValue("RideSettingsName");
            lblEmailTitle.Text = Localize.GetValue("RideSettingsEmailTitle");
            lblPhone.Text = Localize.GetValue("RideSettingsPhone");
            lblVehicleType.Text = Localize.GetValue("RideSettingsVehiculeType");
            lblChargeType.Text = Localize.GetValue("RideSettingsChargeType");
			lblPassword.Text = Localize.GetValue("RideSettingsPassword");
			lblAccountNumber.Text = Localize.GetValue("RideSettingsAccountNumber");
            lblCustomerNumber.Text = Localize.GetValue("RideSettingsCustomerNumber");
            lblTip.Text = Localize.GetValue("PaymentDetails.TipAmountLabel");
			lblPayBack.Text = Localize.GetValue("RideSettingsPayBack");

            txtName.Placeholder = Localize.GetValue("RideSettingsName");
            txtName.AccessibilityLabel = txtName.Placeholder;

            txtPhone.Placeholder = Localize.GetValue("RideSettingsPhone");
            txtPhone.AccessibilityLabel = txtPhone.Placeholder;

            txtVehicleType.Placeholder = Localize.GetValue("RideSettingsVehiculeType");
            txtVehicleType.AccessibilityLabel = txtVehicleType.Placeholder;

            txtChargeType.Placeholder = Localize.GetValue("RideSettingsChargeType");
            txtChargeType.AccessibilityLabel = txtChargeType.Placeholder;

            txtPassword.Placeholder = Localize.GetValue("RideSettingsPassword");
            txtPassword.AccessibilityLabel = txtPassword.Placeholder;

            txtAccountNumber.Placeholder = Localize.GetValue("RideSettingsAccountNumber");
            txtAccountNumber.AccessibilityLabel = txtAccountNumber.Placeholder;

            txtCustomerNumber.Placeholder = Localize.GetValue("RideSettingsCustomerNumber");
            txtCustomerNumber.AccessibilityLabel = txtCustomerNumber.Placeholder;

            txtTip.Placeholder = Localize.GetValue("PaymentDetails.TipAmountLabel");
            txtTip.AccessibilityLabel = txtTip.Placeholder;

            txtPayBack.Placeholder = Localize.GetValue("RideSettingsPayBack");
            txtPayBack.AccessibilityLabel = txtPayBack.Placeholder;

            txtPassword.Text = "\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022";

            DismissKeyboardOnReturn(txtName);

			lblDialCode.Configure(this.NavigationController, (DataContext as RideSettingsViewModel).PhoneNumber);
			lblDialCode.Font = UIFont.FromName(FontName.HelveticaNeueLight, 38/2);
			lblDialCode.TintColor = UIColor.Black;
			lblDialCode.TextColor = UIColor.FromRGB(44, 44, 44);
			lblDialCode.TextAlignment = UITextAlignment.Center;
			lblDialCode.AdjustsFontSizeToFitWidth = true;
			lblDialCode.BackgroundColor = UIColor.White;
			lblDialCode.NotifyChanges += (object sender, PhoneNumberChangedEventArgs e) =>
			{
                    this.ViewModel.SelectedCountryCode = CountryCode.GetCountryCodeByIndex(CountryCode.GetCountryCodeIndexByCountryISOCode(e.Country));
			};

            txtPhone.ShowCloseButtonOnKeyboard();
			txtPayBack.ShowCloseButtonOnKeyboard();

            txtVehicleType.Configure(Localize.GetValue("RideSettingsVehiculeType"), () => ViewModel.Vehicles, () => ViewModel.VehicleTypeId, x => ViewModel.SetVehiculeType.ExecuteIfPossible(x.Id));
            txtChargeType.Configure(Localize.GetValue("RideSettingsChargeType"), () => ViewModel.Payments, () => ViewModel.ChargeTypeId, x => ViewModel.SetChargeType.ExecuteIfPossible(x.Id));
            txtTip.Configure(Localize.GetValue("PaymentDetails.TipAmountLabel"), () => ViewModel.PaymentPreferences.Tips, () => ViewModel.PaymentPreferences.Tip, x => ViewModel.PaymentPreferences.Tip = (int)x.Id, true);
            txtTip.TextAlignment = UITextAlignment.Right;

            NavigationItem.RightBarButtonItem = new UIBarButtonItem(Localize.GetValue("Save"), UIBarButtonItemStyle.Plain, null);

			var set = this.CreateBindingSet<RideSettingsView, RideSettingsViewModel> ();

            set.Bind (NavigationItem.RightBarButtonItem)
                .For ("Clicked")
                .To(vm => vm.SaveCommand);

			set.Bind(txtName)
				.For(v => v.Text)
				.To(vm => vm.Name);

            set.Bind(lblEmail)
                .For(v => v.Text)
                .To(vm => vm.Email);

			set.Bind(txtPhone)
				.For(v => v.Text)
				.To(vm => vm.Phone);

            set.Bind(txtVehicleType)
                .For(v => v.Text)
				.To(vm => vm.VehicleTypeName);

			set.Bind (txtChargeType)
                .For (v => v.Text)
				.To (vm => vm.ChargeTypeName);

			set.Bind (txtChargeType)
				.For (v => v.Enabled)
				.To (vm => vm.IsChargeTypesEnabled);

			set.Bind(txtAccountNumber)
				.For(v => v.Text)
				.To(vm => vm.AccountNumber);

            set.Bind(txtCustomerNumber)
                .For(v => v.Text)
                .To(vm => vm.CustomerNumber);

			set.Bind(txtPassword)
				.For(v => v.NavigateCommand)
				.To(vm => vm.NavigateToUpdatePassword);

            set.Bind(txtTip)
                .For(v => v.Text)
                .To(vm => vm.PaymentPreferences.TipAmount);

			set.Bind(txtPayBack)
				.For(v => v.Text)
				.To(vm => vm.PayBack);

			set.Apply ();       
        }
    }
}

