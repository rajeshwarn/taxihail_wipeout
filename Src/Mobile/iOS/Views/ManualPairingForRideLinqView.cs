using System;
using Foundation;
using UIKit;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Booking.Mobile.Client.Localization;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class ManualPairingForRideLinqView : BaseViewController<ManualPairingForRideLinqViewModel>
	{
		public ManualPairingForRideLinqView()
			: base("ManualPairingForRideLinqView", null)
		{
		}

	    public override void ViewWillAppear(bool animated)
	    {
	        base.ViewWillAppear(animated);

            NavigationController.NavigationBar.Hidden = false;
            NavigationItem.Title = Localize.GetValue("View_RideLinqPair");

            ChangeThemeOfBarStyle();
            ChangeRightBarButtonFontToBold();
	    }

	    public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			FlatButtonStyle.Green.ApplyTo(btnPair);

            lblInstructions.Text = Localize.GetValue("ManualPairingForRideLinQ_Instructions");

            btnPair.SetTitle(Localize.GetValue("ManualPairingForRideLinQ_Pair"), UIControlState.Normal);
            FlatButtonStyle.Green.ApplyTo(btnPair);

			var bindingSet = this.CreateBindingSet<ManualPairingForRideLinqView, ManualPairingForRideLinqViewModel>();

			bindingSet.Bind(PairingCode1)
				.To(vm => vm.PairingCodeLeft);

			bindingSet.Bind(PairingCode2)
				.To(vm => vm.PairingCodeRight);

			bindingSet.Bind(btnPair)
				.For(v => v.Command)
				.To(vm => vm.PairWithRideLinq);

			bindingSet.Apply();

			// Set focus on text box
			PairingCode1.BecomeFirstResponder();

            PairingCode1.ShouldChangeCharacters = (field, range, s) => CheckMaxLength(field, range, s, 3);
            PairingCode2.ShouldChangeCharacters = (field, range, s) => CheckMaxLength(field, range, s, 4);

			PairingCode1.EditingChanged += (sender, e) => 
			{
				if(PairingCode1.Text.Length == 3)
				{
					PairingCode2.BecomeFirstResponder();
				}
			};

			PairingCode2.EditingChanged += (sender, e) => 
			{
				if(PairingCode2.Text.Length == 0)
				{
					PairingCode1.BecomeFirstResponder();
				}
			};
		}

		private bool CheckMaxLength (UITextField textField, NSRange range, string replacementString, nint maxLenght)
		{
			var textLength = textField.Text.HasValue() ? textField.Text.Length : 0;
			var replaceLength = replacementString.HasValue () ? replacementString.Length : 0;
			var newLength = textLength + replaceLength - range.Length;
            return newLength <= maxLenght;
		}
	}
}