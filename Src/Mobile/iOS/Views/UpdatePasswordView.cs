
using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Views;
using Cirrious.MvvmCross.Binding.Touch.Views;
using System.Collections.Generic;
using Cirrious.MvvmCross.Binding.Touch.ExtensionMethods;

namespace apcurium.MK.Booking.Mobile.Client
{
	public partial class UpdatePasswordView : MvxBindingTouchViewController<UpdatePasswordViewModel>
	{
		#region Constructors
		public UpdatePasswordView(Guid accountId) 
			: base(new MvxShowViewModelRequest<UpdatePasswordViewModel>( new Dictionary<string, string>(){{"accountId", accountId.ToString()}}, false, new Cirrious.MvvmCross.Interfaces.ViewModels.MvxRequestedBy()   ) )
		{
		}
		
		public UpdatePasswordView(MvxShowViewModelRequest request) 
			: base(request)
		{
		}
		
		public UpdatePasswordView(MvxShowViewModelRequest request, string nibName, NSBundle bundle) 
			: base(request, nibName, bundle)
		{
		}	
		#endregion
		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			View.BackgroundColor = UIColor.FromPatternImage (UIImage.FromFile ("Assets/background.png"));
			
			lblCurrentPassword.Text = Resources.CurrentPasswordLabel;
			lblNewPassword.Text = Resources.NewPasswordLabel;
			lblNewPasswordConfirmation.Text = Resources.NewPasswordConfirmationLabel;
			lblCurrentPassword.TextColor = AppStyle.TitleTextColor;
			lblNewPassword.TextColor = AppStyle.TitleTextColor;
			lblNewPasswordConfirmation.TextColor = AppStyle.TitleTextColor;
			
			txtCurrentPassword.TextColor = AppStyle.GreyText;
			txtNewPassword.TextColor = AppStyle.GreyText;
			txtNewPasswordConfirmation.TextColor = AppStyle.GreyText;
			
			txtCurrentPassword.PaddingLeft = 5;
			txtNewPassword.PaddingLeft = 5;
			txtNewPasswordConfirmation.PaddingLeft = 5;
			
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
			
			this.AddBindings(new Dictionary<object, string>(){
				{txtCurrentPassword, "{'Text':{'Path':'CurrentPassword'}}"} ,
				{txtNewPassword, "{'Text':{'Path':'NewPassword'}}"} ,
				{txtNewPasswordConfirmation, "{'Text':{'Path':'NewPasswordConfirmation'}}"} ,
			});
            this.View.ApplyAppFont ();
		}
		
		public override void ViewDidUnload ()
		{
			base.ViewDidUnload ();
			
			// Clear any references to subviews of the main view in order to
			// allow the Garbage Collector to collect them sooner.
			//
			// e.g. myOutlet.Dispose (); myOutlet = null;
			
			ReleaseDesignerOutlets ();
		}
		
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			// Return true for supported orientations
			return (toInterfaceOrientation != UIInterfaceOrientation.PortraitUpsideDown);
		}

		private bool ShouldReturnDelegate( UITextField textField )
		{
			return textField.ResignFirstResponder();
		}
	}
}

