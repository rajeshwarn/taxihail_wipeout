using System;
using Foundation;
using UIKit;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.Client.Localization;
using Cirrious.MvvmCross.Binding.BindingContext;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class EditAutoTipView : BaseViewController<EditAutoTipViewModel>
	{
		public EditAutoTipView ()
			: base ("EditAutoTipView", null)
		{
		}

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);

			NavigationController.NavigationBar.Hidden = false;

			ChangeThemeOfBarStyle();
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			View.BackgroundColor = UIColor.FromRGB(242, 242, 242);

			lblTip.Text = Localize.GetValue("PaymentDetails.TipAmountLabel");
			txtTip.Configure(Localize.GetValue("PaymentDetails.TipAmountLabel"), () => ViewModel.PaymentPreferences.Tips, () => ViewModel.PaymentPreferences.Tip, x => ViewModel.PaymentPreferences.Tip = (int)x.Id, true);
            txtTip.TextAlignment = UITextAlignment.Right;

            lblCreditCard.Text = Localize.GetValue("PaymentDetails.PaymentMethodsLabel");
            txtCreditCard.Configure(Localize.GetValue("PaymentDetails.PaymentMethodsLabel"), () => ViewModel.CreditCards, () => ViewModel.CreditCardSelected, x => ViewModel.CreditCardSelected = (int)x.Id);
            txtCreditCard.TextAlignment = UITextAlignment.Right;
            txtCreditCard.Font = UIFont.FromName(FontName.HelveticaNeueLight, 34/2);


			btnSave.SetTitle (Localize.GetValue("Save"), UIControlState.Normal);
			FlatButtonStyle.Green.ApplyTo(btnSave);

			var set = this.CreateBindingSet<EditAutoTipView, EditAutoTipViewModel> ();

			set.Bind(txtTip)
				.For(v => v.Text)
                .To(vm => vm.PaymentPreferences.TipAmount);

            set.Bind(txtCreditCard)
                .For(v => v.Text)
                .To(vm => vm.CreditCardSelectedDisplay);

            set.Bind(txtCreditCard)
                .For(v => v.ImageLeftSource)
                .To(vm => vm.CreditCardSelectedImage);

			set.Bind (btnSave)
				.For(v => v.Command)
                .To(vm => vm.SaveAutoTipChangeCommand);

            set.Bind (NavigationItem)
                .For(v => v.Title)
                .To(vm => vm.ViewTitle);

            set.Bind (this)
                .For(v => v.RemoveCCFromView)
                .To(vm => vm.CanShowCreditCard)
                .WithConversion("BoolInverter");

			set.Apply();
		}

        private bool _removeCCFromView;
        public bool RemoveCCFromView
        {
            get
            {
                return _removeCCFromView;
            }
            set
            {
                _removeCCFromView = value;
                if (RemoveCCFromView)
                {
                    txtCreditCard.RemoveFromSuperview();
                    lblCreditCard.RemoveFromSuperview();
                }
            }
        }
	}
}

