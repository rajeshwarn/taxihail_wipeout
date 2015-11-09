using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.ViewModels;
using UIKit;
using Cirrious.MvvmCross.Binding.BindingContext;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class UpdatePasswordView : BaseViewController<UpdatePasswordViewModel>
	{
		public UpdatePasswordView() 
			: base("UpdatePasswordView", null)
		{
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			NavigationItem.HidesBackButton = false;
			NavigationItem.Title = Localize.GetValue("View_UpdatePassword");
            ChangeRightBarButtonFontToBold();
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

            View.BackgroundColor = UIColor.FromRGB (242, 242, 242);

			lblCurrentPassword.Text = Localize.GetValue("UpdatePasswordCurrentPasswordLabel");
			lblNewPassword.Text = Localize.GetValue("UpdatePasswordNewPasswordLabel");
			lblConfirmation.Text = Localize.GetValue("UpdatePasswordConfirmationLabel");

            ScrollView.ContentSize = new CoreGraphics.CGSize(ScrollView.Frame.Width, ScrollView.Frame.Height);

            txtCurrentPassword.ShouldReturn = ShouldReturnDelegate;
			txtNewPassword.ShouldReturn = ShouldReturnDelegate;
			txtConfirmation.ShouldReturn = ShouldReturnDelegate;

			NavigationItem.RightBarButtonItem = new UIBarButtonItem (Localize.GetValue ("Save"), UIBarButtonItemStyle.Plain, null);

			var set = this.CreateBindingSet<UpdatePasswordView, UpdatePasswordViewModel>();

			set.Bind (NavigationItem.RightBarButtonItem)
				.For ("Clicked")
				.To(vm => vm.UpdateCommand);

			set.Bind(txtCurrentPassword)
				.For(v => v.Text)
				.To(vm => vm.CurrentPassword);

			set.Bind(txtNewPassword)
				.For(v => v.Text)
				.To(vm => vm.NewPassword);

			set.Bind(txtConfirmation)
				.For(v => v.Text)
				.To(vm => vm.Confirmation);

			set.Apply ();
		}

		private bool ShouldReturnDelegate( UITextField textField )
		{
			return textField.ResignFirstResponder();
		}
	}
}

