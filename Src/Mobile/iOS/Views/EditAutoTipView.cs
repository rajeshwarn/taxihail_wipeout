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
			NavigationItem.Title = Localize.GetValue("View_EditAutoTip");

			ChangeThemeOfBarStyle();
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			View.BackgroundColor = UIColor.FromRGB(242, 242, 242);

			lblTip.Text = Localize.GetValue("PaymentDetails.TipAmountLabel");
			txtTip.Configure(Localize.GetValue("PaymentDetails.TipAmountLabel"), () => ViewModel.PaymentPreferences.Tips, () => ViewModel.PaymentPreferences.Tip, x => ViewModel.PaymentPreferences.Tip = (int)x.Id, true);
			txtTip.TextAlignment = UITextAlignment.Right;

			btnSave.SetTitle (Localize.GetValue("Save"), UIControlState.Normal);
			FlatButtonStyle.Green.ApplyTo(btnSave);

			var set = this.CreateBindingSet<EditAutoTipView, EditAutoTipViewModel> ();

			set.Bind(txtTip)
				.For(v => v.Text)
				.To(vm => vm.PaymentPreferences.TipAmount);

			set.Bind (btnSave)
				.For(v => v.Command)
				.To(vm => vm.SaveAutoTipChangeCommand);

			set.Apply();
		}
	}
}

