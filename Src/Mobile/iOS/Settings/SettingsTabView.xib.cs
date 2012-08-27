
using System;
using System.IO;

using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.MessageUI;

using apcurium.Framework.Extensions;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Infrastructure;


namespace apcurium.MK.Booking.Mobile.Client
{
	public partial class SettingsTabView : UIViewController, ITaxiViewController, ISelectableViewController, IRefreshableViewController
	{
		#region Constructors

		// The IntPtr and initWithCoder constructors are required for items that need 
		// to be able to be created from a xib rather than from managed code
		
		
		
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
			
			scrollView.ContentSize = contentView.Frame.Size;

			btnCall.SetTitle (Resources.CallCompanyButton, UIControlState.Normal);
			
			btnTechSupport.SetTitle (Resources.TechSupportButton, UIControlState.Normal);
			btnAbout.SetTitle( Resources.AboutButton , UIControlState.Normal );
			btnChangeSettings.SetTitle (Resources.ChangeDefaultRideSettings, UIControlState.Normal);
			btnSignOut.SetTitle (Resources.SignOutButton, UIControlState.Normal);
			btnError.SetTitle (Resources.SendErrorLogButton, UIControlState.Normal);
			
			btnChangeSettings.TouchUpInside += ChangeSettingsTouchUpInside;
			btnSignOut.TouchUpInside += SignOutTouchUpInside;			
			btnAbout.TouchUpInside += AboutUs;
			btnTechSupport.TouchUpInside += TechSupportTouchUpInside;
						
			btnCall.TouchUpInside += CallTouchUpInside;
			
			
			AppButtons.FormatStandardGradientButton((GradientButton)btnSignOut, Resources.SignOutButton, UIColor.White, AppStyle.ButtonColor.Red); 

						
			imgCreatedBy.Image = UIImage.FromFile ("Assets/apcuriumLogo.png");
			lblLoginStatus.Text = string.Format (Resources.SettingViewLoginInfo, AppContext.Current.LoggedUser.Name);
			lblVersion.Text = string.Format (Resources.Version, AppSettings.Version);
			lblServerVersion.Text = string.Format( Resources.ServerVersion, TinyIoC.TinyIoCContainer.Current.Resolve<IAppSettings>().ServiceServerVersion );
			lblServerName.Text = string.Format( Resources.ServerName, TinyIoC.TinyIoCContainer.Current.Resolve<IAppSettings>().ServiceServerName );
		}
		
		
		
		void AboutUs (object sender, EventArgs e)
		{
			var aboutUs = new AboutUsView(  );
			
			this.NavigationController.PushViewController (aboutUs, true);
		}


		public void Selected ()
		{
			lblLoginStatus.Maybe (() => lblLoginStatus.Text = string.Format (Resources.SettingViewLoginInfo, AppContext.Current.LoggedUser.Name));
			lblServerVersion.Maybe (() => lblServerVersion.Text = string.Format( Resources.ServerVersion, TinyIoC.TinyIoCContainer.Current.Resolve<IAppSettings>().ServiceServerVersion ));
			lblServerName.Maybe (() => lblServerName.Text = string.Format( Resources.ServerName, TinyIoC.TinyIoCContainer.Current.Resolve<IAppSettings>().ServiceServerName ));
		}


		public void RefreshData ()
		{
			lblLoginStatus.Maybe (() => lblLoginStatus.Text = string.Format (Resources.SettingViewLoginInfo, AppContext.Current.LoggedUser.Name));
			lblServerVersion.Maybe (() => lblServerVersion.Text = string.Format( Resources.ServerVersion, TinyIoC.TinyIoCContainer.Current.Resolve<IAppSettings>().ServiceServerVersion ));
			lblServerName.Maybe (() => lblServerName.Text = string.Format( Resources.ServerName, TinyIoC.TinyIoCContainer.Current.Resolve<IAppSettings>().ServiceServerName ));
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
			
			_mailComposer.SetToRecipients (new string[] { "techsupport@apcurium.com" });
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

			call.Call ( AppSettings.PhoneNumber(AppContext.Current.LoggedUser.Settings.ProviderId), AppSettings.PhoneNumberDisplay (AppContext.Current.LoggedUser.Settings.ProviderId));
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
					_mailComposer.SetMessageBody ("iOS Error log ", false);
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
			var settings = new RideSettingsView (AppContext.Current.LoggedUser.Settings, true, false);

			settings.Closed += delegate { 
				ThreadHelper.ExecuteInThread (() => {
					InvokeOnMainThread (() => {						
						btnCall.SetTitle (Resources.CallCompanyButton, UIControlState.Normal);});					 						
					});
			};
			
			this.NavigationController.PushViewController (settings, true);
			
		}
		
		
		#endregion
	}
}

