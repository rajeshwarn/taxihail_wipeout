using System;
using System.Text.RegularExpressions;
using System.Linq;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using apcurium.Framework.Extensions;
using apcurium.Framework;
using Microsoft.Practices.ServiceLocation;
namespace TaxiMobileApp
{
	public class ResetPasswordView : DialogViewController
	{
		private EntryElement _emailEntry;
		private ResetPasswordData _data;

		public ResetPasswordView () : base(null)
		{
		}

		public override void ViewDidLoad ()
		{
			_data = new ResetPasswordData ();
			LoadSettingsElements ();
			
			
			AddButton (170, 100, Resources.ResetPasswordReset, AppStyle.AcceptButtonColor, AppStyle.AcceptButtonHighlightedColor, () => ResetPassword ());
			AddButton (20, 100, Resources.ResetPasswordCancel, AppStyle.LightButtonColor, AppStyle.LightButtonHighlightedColor, () => Cancel ());
		}


		private void ResetPassword ()
		{
			_emailEntry.FetchValue ();
			
			
			
			if (!IsEmail (_data.Email))
			{
				MessageHelper.Show (Resources.ResetPasswordInvalidDataTitle, Resources.ResetPasswordInvalidDataMessage);
				return;
			}
			
			
			
			LoadingOverlay.StartAnimatingLoading (this.View, LoadingOverlayPosition.Center, null, 130, 30);
			ThreadHelper.ExecuteInThread (() =>
			{
				try
				{
					var result = ServiceLocator.Current.GetInstance<IAccountService> ().ResetPassword (_data);
					if (result)
					{
						InvokeOnMainThread (() => MessageHelper.Show (Resources.ResetPasswordConfirmationTitle, Resources.ResetPasswordConfirmationMessage) );
						BeginInvokeOnMainThread (() => this.DismissModalViewControllerAnimated (true));
					}					
					else
					{
						BeginInvokeOnMainThread (() => MessageHelper.Show (Resources.NoConnectionTitle, Resources.NoConnectionMessage));
					}
				}
				finally
				{
					InvokeOnMainThread (() => this.View.UserInteractionEnabled = true);
					LoadingOverlay.StopAnimatingLoading (this.View);
				}
			});
			
			
			
			
		}
		
		public override string GetTitle ()
		{
			return Resources.ResetPasswordTitle;
		}
		
		public static bool IsEmail (string inputEmail)
		{
			inputEmail = inputEmail.ToSafeString ();
			string strRegex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" + @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" + @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
			Regex re = new Regex (strRegex);
			if (re.IsMatch (inputEmail))
				return (true);
			else
				return (false);
		}

		private void Cancel ()
		{
			this.DismissModalViewControllerAnimated (true);
		}

		public override void ViewDidAppear (bool animated)
		{
			
		}


		public override void LoadView ()
		{
			base.LoadView ();
			
		}

		public override UIColor GetCellColor ()
		{
			return UIColor.White;
		}
		private void LoadSettingsElements ()
		{
			
			
			var menu = new RootElement (Resources.ResetPasswordTitle);
			
			var settings = new Section (Resources.ResetPasswordTitle);
			
			
			_emailEntry = new EntryElement (Resources.ResetPasswordLabel, Resources.ResetPasswordPlaceholder, _data.Email);
			_emailEntry.KeyboardType = MonoTouch.UIKit.UIKeyboardType.EmailAddress;
			_emailEntry.TextAutocapitalizationType = UITextAutocapitalizationType.None;
			_emailEntry.TextAutocorrectionType = UITextAutocorrectionType.No;
			
			_emailEntry.Changed += delegate {
				_emailEntry.FetchValue ();
				_data.Email = _emailEntry.Value;
			};
			
			menu.Add (settings);
			settings.Add (_emailEntry);
			
			
			
			this.InvokeOnMainThread (() => { this.Root = menu; });
			
			
		}


		private void AddButton (float x, float y, string title, UIColor normal, UIColor selected, Action clicked)
		{
			var btn = UIButton.FromType (UIButtonType.RoundedRect);
			btn.Frame = new System.Drawing.RectangleF (x, y, 130, 40);
			btn.SetTitle (title, UIControlState.Normal);
			this.View.AddSubview (btn);
			GlassButton.Wrap (btn, normal, selected).TouchUpInside += delegate { clicked (); };
			
			//btn.TouchUpInside += delegate { clicked (); };
			
		}
		
		
		
	}
}

