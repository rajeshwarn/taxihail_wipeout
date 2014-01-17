using System;
using System.Collections.Generic;
using System.Drawing;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Views;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
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
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			View.BackgroundColor = UIColor.FromRGB (239,239,239);

			lblCurrentPassword.Text = Localize.GetValue("UpdatePasswordCurrentPasswordLabel");
			lblNewPassword.Text = Localize.GetValue("UpdatePasswordNewPasswordLabel");
			lblConfirmation.Text = Localize.GetValue("UpdatePasswordConfirmationLabel");
			
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

