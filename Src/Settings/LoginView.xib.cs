
using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.Framework.Extensions;
using Microsoft.Practices.ServiceLocation;
using MonoTouch.MessageUI;
using System.IO;

namespace TaxiMobileApp
{
	public class txtDel : UITextFieldDelegate
	{
		
		
	}

	public partial class LoginView : UIViewController
	{
		#region Constructors

		// The IntPtr and initWithCoder constructors are required for items that need 
		// to be able to be created from a xib rather than from managed code

		public LoginView (IntPtr handle) : base(handle)
		{
			Initialize ();
		}

		[Export("initWithCoder:")]
		public LoginView (NSCoder coder) : base(coder)
		{
			Initialize ();
		}

		public LoginView () : base("LoginView", null)
		{
			Initialize ();
		}

		void Initialize ()
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			View.BackgroundColor = UIColor.FromPatternImage (UIImage.FromFile ("Assets/background_full.png"));
			//lblTitle.Text = Resources.SignInViewTitle;
			lblEmail.Text = Resources.EmailLabel;
			
			if (AppContext.Current.LastEmailUsed.HasValue ())
			{
				txtEmail.Text = AppContext.Current.LastEmailUsed;
			}
			
			lblPassword.Text = Resources.PasswordLabel;
			
			btnSignIn.SetTitle (Resources.SignInButton, UIControlState.Normal);
			
			lblCreateAccount.Text = Resources.SignUpButton;
			lblForgotPassword.Text = Resources.LoginForgotPasswordButton;
			btnPassword.TouchUpInside += PasswordTouchUpInside;			
			txtEmail.ReturnKeyType = UIReturnKeyType.Done;
			txtEmail.AutocapitalizationType = UITextAutocapitalizationType.None;
			txtEmail.AutocorrectionType = UITextAutocorrectionType.No;
			txtEmail.KeyboardType = UIKeyboardType.EmailAddress;
			txtEmail.ShouldReturn = delegate(UITextField textField) { return textField.ResignFirstResponder (); };
			
			txtPassword.SecureTextEntry = true;
			txtPassword.ReturnKeyType = UIReturnKeyType.Done;
			txtPassword.ShouldReturn = delegate(UITextField textField) { return textField.ResignFirstResponder (); };
			
			txtEmail.EditingDidEnd += delegate { txtEmail.Text = StringHelper.RemoveDiacritics (txtEmail.Text).ToLower (); };
			
			
			btnSignUp.TouchUpInside += SignUpClicked;
			
			GlassButton.Wrap( btnSignIn, AppStyle.AcceptButtonColor, AppStyle.AcceptButtonHighlightedColor ).TouchUpInside += SignInClicked;
			
			txtEmail.BecomeFirstResponder ();
		}



		void SignUpClicked (object sender, EventArgs e)
		{
			
			//UIApplication.SharedApplication.OpenUrl (new NSUrl (AppSettings.SiteUrl + "Client.aspx"));
			
			var view =new CreateAccountView ();
			view.AccountCreated += delegate(object data, EventArgs e2) {
				if ( data is CreateAccountData )
				{
					InvokeOnMainThread( () => txtEmail.Text = ((CreateAccountData )data).Email);
				}
			};
			
			var nav = new UINavigationController(  view  );
			nav.NavigationBar.BarStyle = UIBarStyle.Black;
			nav.Title = Resources.CreateAccoutTitle;	
			this.PresentModalViewController (nav, true);
		}

		void PasswordTouchUpInside (object sender, EventArgs e)
		{
			
			
			var nav = new UINavigationController(  new ResetPasswordView () );
			nav.NavigationBar.BarStyle = UIBarStyle.Black;
			nav.Title = Resources.ResetPasswordTitle;	
			this.PresentModalViewController (nav, true);
		}

		void SignInClicked (object sender, EventArgs e)
		{
			
			
			
			try
			{
				AppContext.Current.SignOutUser();
				LoadingOverlay.StartAnimatingLoading (this.View, LoadingOverlayPosition.Center, null, 130, 30);
				ThreadHelper.ExecuteInThread (() =>
				{
					try
					{
						
						InvokeOnMainThread (() => this.View.UserInteractionEnabled = false);
						var service = ServiceLocator.Current.GetInstance<IAccountService> ();
						string error = "";
						var account = service.GetAccount (txtEmail.Text, txtPassword.Text, out error);
						if (account != null)
						{
							AppContext.Current.LastEmailUsed = txtEmail.Text;
							InvokeOnMainThread (() => AppContext.Current.UpdateLoggedInUser (account, false));
							InvokeOnMainThread (() => this.DismissModalViewControllerAnimated (true));
							
							if (AppContext.Current.Controller.SelectedRefreshableViewController != null)
							{
								AppContext.Current.Controller.View.InvokeOnMainThread (() => {AppContext.Current.Controller.RefreshCompanyButtons(); });
								AppContext.Current.Controller.View.InvokeOnMainThread (() => { AppContext.Current.Controller.SelectedRefreshableViewController.RefreshData (); });
							}
							
						}
												
						else
						{
							MessageHelper.Show (Resources.InvalidLoginMessageTitle, Resources.InvalidLoginMessage + " (" + error + ")");
						}
						
					}
					finally
					{
						InvokeOnMainThread (() => this.View.UserInteractionEnabled = true);
						LoadingOverlay.StopAnimatingLoading (this.View);
					}
				});
			}
			finally
			{
				
			}
			
		}
		#endregion
	}
}

