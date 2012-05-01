using System;
using System.IO;
using MonoTouch.Foundation;
using MonoTouch.MessageUI;
using MonoTouch.UIKit;
using TaxiMobile.Book;
using TaxiMobile.Controls;
using TaxiMobile.Helper;
using TaxiMobile.Lib.Framework.Extensions;
using TaxiMobile.Localization;

namespace TaxiMobile.Settings
{
	public partial class SettingsTabView : UIViewController, ITaxiViewController, ISelectableViewController, IRefreshableViewController
	{
		#region Constructors

		// The IntPtr and initWithCoder constructors are required for items that need 
		// to be able to be created from a xib rather than from managed code
		
		private UIButton _btnGlossyCall;
		
		public SettingsTabView (IntPtr handle) : base(handle)
		{
			Initialize ();
		}

		[Export("initWithCoder:")]
		public SettingsTabView (NSCoder coder) : base(coder)
		{
			Initialize ();
		}

		public SettingsTabView () : base("SettingsTabView", null)
		{
			Initialize ();
		}

		void Initialize ()
		{
		}

		public UIView GetTopView ()
		{
			return null;
		}

		public string GetTitle ()
		{
			return Resources.SettingsViewTitle;
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			View.BackgroundColor = UIColor.FromPatternImage (UIImage.FromFile ("Assets/background.png"));
			
			var callBtnTitle = string.Format (Resources.CallCompanyButton, AppContext.Current.LoggedUser.DefaultSettings.CompanyName);
			btnCall.SetTitle (callBtnTitle, UIControlState.Normal);
			
			btnTechSupport.SetTitle (Resources.TechSupportButton, UIControlState.Normal);
			btnAbout.SetTitle( Resources.AboutButton , UIControlState.Normal );
			btnChangeSettings.SetTitle (Resources.ChangeDefaultRideSettings, UIControlState.Normal);
			btnSignOut.SetTitle (Resources.SignOutButton, UIControlState.Normal);
			btnError.SetTitle (Resources.SendErrorLogButton, UIControlState.Normal);
			
			GlassButton.Wrap(btnChangeSettings, AppStyle.LightButtonColor , AppStyle.LightButtonHighlightedColor ).TouchUpInside += ChangeSettingsTouchUpInside;
			GlassButton.Wrap(btnSignOut, AppStyle.LightButtonColor , AppStyle.LightButtonHighlightedColor ).TouchUpInside += SignOutTouchUpInside;
			//GlassButton.Wrap(btnError, AppStyle.LightButtonColor , AppStyle.LightButtonHighlightedColor ).TouchUpInside += SendLog;
			GlassButton.Wrap(btnAbout, AppStyle.LightButtonColor , AppStyle.LightButtonHighlightedColor ).TouchUpInside += AboutUs;
			GlassButton.Wrap(btnTechSupport, AppStyle.LightButtonColor , AppStyle.LightButtonHighlightedColor ).TouchUpInside += TechSupportTouchUpInside;
			
			_btnGlossyCall = GlassButton.Wrap( btnCall , AppStyle.LightButtonColor , AppStyle.LightButtonHighlightedColor );
			_btnGlossyCall.TouchUpInside += CallTouchUpInside;
			
			
			
						
			imgCreatedBy.Image = UIImage.FromFile ("Assets/apcuriumLogo.png");
			lblLoginStatus.Text = string.Format (Resources.SettingViewLoginInfo, AppContext.Current.LoggedUser.DisplayName);
			lblVersion.Text = string.Format (Resources.Version, AppSettings.Version);
			
		}
		
		
		
		void AboutUs (object sender, EventArgs e)
		{
			var aboutUs = new AboutUsView(  );
			
			this.NavigationController.PushViewController (aboutUs, true);
		}


		public void Selected ()
		{
			lblLoginStatus.Maybe (() => lblLoginStatus.Text = string.Format (Resources.SettingViewLoginInfo, AppContext.Current.LoggedUser.DisplayName));
		}


		public void RefreshData ()
		{
			lblLoginStatus.Maybe (() => lblLoginStatus.Text = string.Format (Resources.SettingViewLoginInfo, AppContext.Current.LoggedUser.DisplayName));
		}
		void TechSupportTouchUpInside (object sender, EventArgs e)
		{
			
			if (!MFMailComposeViewController.CanSendMail)
			{
				return;
			}
			
			_mailComposer = new MFMailComposeViewController ();
			
			if (File.Exists (AppSettings.ErrorLog))
			{
				_mailComposer.AddAttachmentData (NSData.FromFile (AppSettings.ErrorLog), "text", "errorlog.txt");
			}
			
			_mailComposer.SetToRecipients (new string[] { "support.iphone@taxidiamond.com" });
			_mailComposer.SetMessageBody ("", false);
			_mailComposer.SetSubject (Resources.TechSupportButton);
			_mailComposer.Finished += delegate(object mailsender, MFComposeResultEventArgs mfce) {
				_mailComposer.DismissModalViewControllerAnimated (true);
				if (File.Exists (AppSettings.ErrorLog))
				{
					File.Delete (AppSettings.ErrorLog);
				}
			};
			
			PresentModalViewController (_mailComposer, true);
		}

		void CallTouchUpInside (object sender, EventArgs e)
		{
			var call = new Confirmation ();
			call.Call (AppSettings.PhoneNumber (AppContext.Current.LoggedUser.DefaultSettings.Company), AppSettings.PhoneNumberDisplay (AppContext.Current.LoggedUser.DefaultSettings.Company));
		}

		private MFMailComposeViewController _mailComposer;
		void SendLog (object sender, EventArgs e)
		{
			
			if (File.Exists (AppSettings.ErrorLog))
			{
				
				
				if (MFMailComposeViewController.CanSendMail)
				{
					_mailComposer = new MFMailComposeViewController ();
					
					
					_mailComposer.AddAttachmentData (NSData.FromFile (AppSettings.ErrorLog), "text", "errorlog.txt");
					_mailComposer.SetMessageBody ("Error log : " + UIDevice.CurrentDevice.Model, false);
					_mailComposer.SetSubject ("Error log");
					_mailComposer.Finished += delegate(object mailsender, MFComposeResultEventArgs mfce) {
						_mailComposer.DismissModalViewControllerAnimated (true);
						File.Delete (AppSettings.ErrorLog);
					};
					
					PresentModalViewController (_mailComposer, true);
				}
				
			}
			
		}

		void SignOutTouchUpInside (object sender, EventArgs e)
		{
			AppContext.Current.SignOutUser ();
			
			AppContext.Current.WarnEstimate = true;
			
			this.PresentModalViewController (new LoginView (), true);
		}

		void ChangeSettingsTouchUpInside (object sender, EventArgs e)
		{
			var settings = new RideSettingsView (AppContext.Current.LoggedUser.DefaultSettings, true, false);
			



			settings.Closed += delegate { 
				
				ThreadHelper.ExecuteInThread (() =>{InvokeOnMainThread (() =>{
						
						var callBtnTitle = string.Format (Resources.CallCompanyButton, settings.Result.CompanyName);
						_btnGlossyCall.SetTitle (callBtnTitle, UIControlState.Normal);});					 
						AppContext.Current.Controller.RefreshCompanyButtons();
					
				
				}); };
			
			this.NavigationController.PushViewController (settings, true);
			
		}
		
		
		#endregion
	}
}

