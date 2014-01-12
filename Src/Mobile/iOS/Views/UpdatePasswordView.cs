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
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
            scrollView.ContentSize = new SizeF(scrollView.ContentSize.Width, txtNewPassword.Frame.Bottom + 200);
            NavigationItem.Title = Localize.GetValue("Password");
			View.BackgroundColor = UIColor.FromPatternImage (UIImage.FromFile ("Assets/background.png"));

            lblCurrentPassword.Text = Localize.GetValue("CurrentPasswordLabel");
            lblNewPassword.Text = Localize.GetValue("NewPasswordLabel");
            lblNewPasswordConfirmation.Text = Localize.GetValue("NewPasswordConfirmationLabel");
			
            txtCurrentPassword.ShouldReturn = ShouldReturnDelegate;
			txtNewPassword.ShouldReturn = ShouldReturnDelegate;
			txtNewPasswordConfirmation.ShouldReturn = ShouldReturnDelegate;

            var btnDone = new UIBarButtonItem(Localize.GetValue("DoneButton"), UIBarButtonItemStyle.Plain, delegate
            {
				if( ViewModel.UpdateCommand.CanExecute() )
				{
					ViewModel.UpdateCommand.Execute();
				}
			});
			NavigationItem.HidesBackButton = false;
			NavigationItem.RightBarButtonItem = btnDone;
			
			this.AddBindings(new Dictionary<object, string>{
				{txtCurrentPassword, "{'Text':{'Path':'CurrentPassword'}}"} ,
				{txtNewPassword, "{'Text':{'Path':'NewPassword'}}"} ,
				{txtNewPasswordConfirmation, "{'Text':{'Path':'NewPasswordConfirmation'}}"} ,
			});
            View.ApplyAppFont ();
		}

		private bool ShouldReturnDelegate( UITextField textField )
		{
			return textField.ResignFirstResponder();
		}
	}
}

