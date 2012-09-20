
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
	public partial class SettingsTabView : UIViewController
	{
		#region Constructors

		// The IntPtr and initWithCoder constructors are required for items that need 
		// to be able to be created from a xib rather than from managed code
		
		
		
		public SettingsTabView (IntPtr handle) : base(handle)
		{
		}

		[Export("initWithCoder:")]
		public SettingsTabView (NSCoder coder) : base(coder)
		{
		}

		public SettingsTabView () : base("SettingsTabView", null)
		{
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

			lblLoginStatus.Text = Resources.SettingViewLoginInfo;
			lblLoginStatus.TextColor = AppStyle.TitleTextColor;
			lblLoggedInUser.Text = "";
			lblLoggedInUser.TextColor = AppStyle.TitleTextColor;

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
			
			
			AppButtons.FormatStandardButton((GradientButton)btnSignOut, Resources.SignOutButton, AppStyle.ButtonColor.Red); 

						
			imgCreatedBy.Image = UIImage.FromFile ("Assets/apcuriumLogo.png");
			lblVersion.TextColor = AppStyle.TitleTextColor;

            lblVersion.Text = string.Format (Resources.Version, TinyIoCContainer.Current.Resolve<IPackageInfo>().Version, AppContext.Current.ServerName, AppContext.Current.ServerVersion );

			UpdateTextFields();
		}
		
		
		
		void AboutUs (object sender, EventArgs e)
		{
			var aboutUs = new AboutUsView(  );
			
			this.NavigationController.PushViewController (aboutUs, true);
		}

		private void UpdateTextFields()
		{
			lblLoggedInUser.Maybe (() => lblLoggedInUser.Text = AppContext.Current.LoggedUser.Name );
            lblVersion.Maybe( () => lblVersion.Text = string.Format (Resources.Version, TinyIoCContainer.Current.Resolve<IPackageInfo>().Version, AppContext.Current.ServerName, AppContext.Current.ServerVersion ) );
		}

		void TechSupportTouchUpInside (object sender, EventArgs e)
		{
			
			if (!MFMailComposeViewController.CanSendMail)
			{
				return;
			}
			
			_mailComposer = new MFMailComposeViewController ();
			
            if (File.Exists (TinyIoCContainer.Current.Resolve<IAppSettings>().ErrorLog))
			{
                _mailComposer.AddAttachmentData (NSData.FromFile (TinyIoCContainer.Current.Resolve<IAppSettings>().ErrorLog), "text", "errorlog.txt");
			}
			
            _mailComposer.SetToRecipients (new string[] { TinyIoCContainer.Current.Resolve<IAppSettings>().SupportEmail  });
			_mailComposer.SetMessageBody ("", false);
			_mailComposer.SetSubject (Resources.TechSupportButton);
			_mailComposer.Finished += delegate(object mailsender, MFComposeResultEventArgs mfce) {
				_mailComposer.DismissModalViewControllerAnimated (true);
                if (File.Exists (TinyIoCContainer.Current.Resolve<IAppSettings>().ErrorLog))
				{
                    File.Delete (TinyIoCContainer.Current.Resolve<IAppSettings>().ErrorLog);
				}
			};
			
			PresentModalViewController (_mailComposer, true);
		}

		void CallTouchUpInside (object sender, EventArgs e)
		{
            if ( !AppContext.Current.LoggedUser.Settings.ProviderId.HasValue )
            {
                return;
            }
			var call = new Confirmation ();

            call.Call ( TinyIoCContainer.Current.Resolve<IAppSettings>().PhoneNumber(AppContext.Current.LoggedUser.Settings.ProviderId.Value),
                       TinyIoCContainer.Current.Resolve<IAppSettings>().PhoneNumberDisplay (AppContext.Current.LoggedUser.Settings.ProviderId.Value));
		}

		private MFMailComposeViewController _mailComposer;
		void SendLog (object sender, EventArgs e)
		{
			
            if (File.Exists (TinyIoCContainer.Current.Resolve<IAppSettings>().ErrorLog))
			{
				
				
				if (MFMailComposeViewController.CanSendMail)
				{
					_mailComposer = new MFMailComposeViewController ();
					
					
                    _mailComposer.AddAttachmentData (NSData.FromFile (TinyIoCContainer.Current.Resolve<IAppSettings>().ErrorLog), "text", "errorlog.txt");
					_mailComposer.SetMessageBody ("iOS Error log ", false);
					_mailComposer.SetSubject ("Error log");
					_mailComposer.Finished += delegate(object mailsender, MFComposeResultEventArgs mfce) {
						_mailComposer.DismissModalViewControllerAnimated (true);
                        File.Delete (TinyIoCContainer.Current.Resolve<IAppSettings>().ErrorLog);
					};
					
					PresentModalViewController (_mailComposer, true);
				}
				
			}
			
		}

		void SignOutTouchUpInside (object sender, EventArgs e)
		{
			AppContext.Current.SignOut ();
			
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

