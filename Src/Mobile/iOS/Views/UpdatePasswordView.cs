using System;
using System.Collections.Generic;
using System.Drawing;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Views;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class UpdatePasswordView : BaseViewController
	{
		public UpdatePasswordView() 
			: base("UpdatePasswordView", null)
		{
		}	
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
            scrollView.ContentSize = new SizeF(scrollView.ContentSize.Width, txtNewPassword.Frame.Bottom + 200);
            NavigationItem.Title = Resources.GetValue("Password");
			View.BackgroundColor = UIColor.FromPatternImage (UIImage.FromFile ("Assets/background.png"));
			
			lblCurrentPassword.Text = Resources.CurrentPasswordLabel;
			lblNewPassword.Text = Resources.NewPasswordLabel;
			lblNewPasswordConfirmation.Text = Resources.NewPasswordConfirmationLabel;
			
            txtCurrentPassword.ShouldReturn = ShouldReturnDelegate;
			txtNewPassword.ShouldReturn = ShouldReturnDelegate;
			txtNewPasswordConfirmation.ShouldReturn = ShouldReturnDelegate;
			
			var btnDone = new UIBarButtonItem (Resources.DoneButton, UIBarButtonItemStyle.Plain, delegate {
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

