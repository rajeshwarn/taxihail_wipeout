using System.Collections.Generic;
using Cirrious.MvvmCross.Binding.BindingContext;
using CrossUI.Touch.Dialog.Elements;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.Client.Style;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class CreateAccountView : BaseViewController<CreateAccountViewModel>
    {
		public CreateAccountView() : base("CreateAccountView", null)
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

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();

            View.BackgroundColor = Theme.LoginColor;
            lblTitle.TextColor = Theme.GetTextColor(Theme.LoginColor);

			if (ViewModel.Settings.ShowTermsAndConditions)
            {
                var textFirstPart = Localize.GetValue("TermsAndConditionsAcknowledgment")
                    .Replace(Localize.GetValue("TermsAndConditionsLabel"), string.Empty)
                    .Replace(".", string.Empty);
                var textSecondPart = string.Format("{0}.", Localize.GetValue("TermsAndConditionsLabel"));
                var attributedText = new NSMutableAttributedString(textFirstPart, foregroundColor: Theme.GetTextColor(Theme.LoginColor), font: UIFont.FromName(FontName.HelveticaNeueRegular, 28 / 2));
                attributedText.Append(new NSMutableAttributedString(textSecondPart, foregroundColor: Theme.GetTextColor(Theme.LoginColor), font: UIFont.FromName(FontName.HelveticaNeueBold, 28 / 2)));
                btnViewTerms.SetAttributedTitle(attributedText, UIControlState.Normal);
            }
            else
            {
                checkBoxAcknowledged.RemoveFromSuperview();
                btnViewTerms.RemoveFromSuperview();
            }

			FlatButtonStyle.Main.ApplyTo (btnCreate);
            btnCreate.SetTitleColor(Theme.GetTextColor(Theme.LoginColor), UIControlState.Normal);
            btnCancel.SetTitleColor(Theme.GetTextColor(Theme.LoginColor), UIControlState.Normal);

			lblTitle.Text = Localize.GetValue ("View_CreateAccount");
			btnCreate.SetTitle (Localize.GetValue ("Create"), UIControlState.Normal);
			btnCancel.SetTitle (Localize.GetValue ("Cancel"), UIControlState.Normal);

			BuildTableView ();

			var set = this.CreateBindingSet<CreateAccountView, CreateAccountViewModel>();

            set.Bind(checkBoxAcknowledged)
                .For(v => v.Selected)
                .To(vm => vm.TermsAndConditionsAcknowledged);

            set.Bind(btnViewTerms)
                .For("TouchUpInside")
                .To(vm => vm.NavigateToTermsAndConditions);

			set.Bind(btnCancel)
				.For("TouchUpInside")
				.To(vm => vm.CloseCommand);

			set.Bind(btnCreate)
				.For("TouchUpInside")
				.To(vm => vm.CreateAccount);
            set.Bind(btnCreate)
                .For(v => v.Enabled)
                .To(vm => vm.TermsAndConditionsAcknowledged);

			set.Apply ();
        }

		private void BuildTableView()
		{
			var bindings = this.CreateInlineBindingTarget<CreateAccountViewModel>();

			var section = new Section () {
				new TaxiHailEntryElement (string.Empty, Localize.GetValue ("CreateAccountEmailPlaceHolder"), ViewModel.Data.Email){ KeyboardType = UIKeyboardType.EmailAddress }
					.Bind(bindings, vm => vm.Data.Email),
					new TaxiHailEntryElement (string.Empty, Localize.GetValue ("CreateAccountFullNamePlaceHolder"), ViewModel.Data.Name, false , UITextAutocapitalizationType.Words)
					.Bind(bindings, vm => vm.Data.Name),
				new TaxiHailEntryElement (string.Empty, Localize.GetValue ("CreateAccountPhonePlaceHolder"), ViewModel.Data.Phone){ KeyboardType = UIKeyboardType.PhonePad }
					.Bind(bindings, vm => vm.Data.Phone)
			};

			if (!ViewModel.HasSocialInfo) {
				constraintTableViewHeight.Constant += 40*2;
				section.AddAll (new List<Element> { 
					new TaxiHailEntryElement (string.Empty, Localize.GetValue ("CreateAccountPasswordPlaceHolder"), ViewModel.Data.Password, true)
						.Bind(bindings, vm => vm.Data.Password), 
					new TaxiHailEntryElement (string.Empty, Localize.GetValue ("CreateAccountPasswordConfirmationPlaceHolder"), ViewModel.ConfirmPassword, true)
						.Bind(bindings, vm => vm.ConfirmPassword)
				});
			}

			var root = new RootElement(){ section };

			tableView.BackgroundColor = UIColor.Clear;
            tableView.AddSubview(new TaxiHailDialogViewController (root, true).TableView);
		}
    }
}

