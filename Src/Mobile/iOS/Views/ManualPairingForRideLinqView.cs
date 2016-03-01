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
        private const int PairingCode1MaxLength = 3;
        private const int PairingCode2MaxLength = 4;


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

			// to hide keyboard when touch
			btnPair.TouchUpInside += (object sender, EventArgs e) => { 
				PairingCode1.ResignFirstResponder(); 
				PairingCode2.ResignFirstResponder();
			};

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

            PairingCode1.ShouldChangeCharacters = (field, range, s) => CheckMaxLength(field, range, s, PairingCode1MaxLength, MoveToPairingCode2);
            PairingCode2.ShouldChangeCharacters = (field, range, s) => CheckMaxLength(field, range, s, PairingCode2MaxLength);

            PairingCode1.EditingChanged += (sender, e) => 
			{
                if (PairingCode1.Text.Length == PairingCode1MaxLength)
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

        private bool CheckMaxLength (UITextField textField, NSRange range, string replacementString, nint maxLenght, Action<string> onMaxLengthAction = null)
		{
			var textLength = textField.Text.HasValue() ? textField.Text.Length : 0;
			var replaceLength = replacementString.HasValue () ? replacementString.Length : 0;
			var newLength = textLength + replaceLength - range.Length;

            if(newLength <= maxLenght)
            {
                return true;
            }

            if(onMaxLengthAction != null)
            {
                onMaxLengthAction(replacementString);
            }

            return false;
		}

        private void MoveToPairingCode2(string value)
        {
            var length = value.HasValue() ? value.Length : 0;

            var caretPosition = PairingCode1.GetOffsetFromPosition(PairingCode1.BeginningOfDocument, PairingCode1.SelectedTextRange.Start);

            var isCaretAtEnd = caretPosition == PairingCode1MaxLength;

            if (PairingCode2.Text.Length + length <= PairingCode2MaxLength && isCaretAtEnd)
            {
                PairingCode2.Text = value + PairingCode2.Text;
                PairingCode2.BecomeFirstResponder();
            }
        }
	}
}