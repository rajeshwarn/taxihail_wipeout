using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Linq;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using apcurium.Framework.Extensions;
using apcurium.Framework;
using Microsoft.Practices.ServiceLocation;

namespace TaxiMobileApp
{
	public class CreateAccountView : DialogViewController
	{
		private CreateAccountData _data;
		private EntryElement _emailEntry;
		private EntryElement _firstNameEntry;
		private EntryElement _lastNameEntry;
		private RootElement _title;
		private EntryElement _phoneEntry;
		private EntryElement _mobileEntry;
		private EntryElement _passwordEntry;
		private EntryElement _confirmPasswordEntry;


		private bool _elementAreLoaded = false;
		
		public event EventHandler AccountCreated;
		
		public CreateAccountView () : base(null)
		{
			_data = new CreateAccountData ();
			_elementAreLoaded = false;
		}

		public CreateAccountView ( CreateAccountData data ) : base(null)
		{
			_data = data;
			_elementAreLoaded = false;
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			this.InvokeOnMainThread( () => NavigationItem.Title = " " );
		}
		public override void ViewDidAppear (bool animated)
		{
			LoadSettingsElements ();
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
		}

		public override string GetTitle ()
		{
			return "";
		//	return Resources.CreateAccoutTitle;
		}
		private void CreateAccount ()
		{
			Console.WriteLine( "CreateAccount" );
			_emailEntry.FetchValue ();
			_firstNameEntry.FetchValue ();
			_lastNameEntry.FetchValue ();
			_phoneEntry.FetchValue ();
			_mobileEntry.FetchValue ();
			_passwordEntry.FetchValue ();
			_confirmPasswordEntry.FetchValue ();
			
			
			_data.Email =  _emailEntry.Value;
			_data.FirstName =  _firstNameEntry.Value;
			_data.LastName =  _lastNameEntry.Value;
			_data.Phone =  _phoneEntry.Value;
			_data.Mobile =  _mobileEntry.Value;
			_data.Password =  _passwordEntry.Value;
			_data.Confirm =  _confirmPasswordEntry.Value;
			
			if (!IsEmail (_data.Email))
			{
				MessageHelper.Show (Resources.ResetPasswordInvalidDataTitle, Resources.ResetPasswordInvalidDataMessage);
				return;
			}
			
			if ( _data.Email.IsNullOrEmpty() || _data.Title.IsNullOrEmpty() || _data.FirstName.IsNullOrEmpty() || _data.LastName.IsNullOrEmpty() || _data.Phone.IsNullOrEmpty() || _data.Password.IsNullOrEmpty() || _data.Confirm.IsNullOrEmpty() )
			{
				MessageHelper.Show (Resources.CreateAccountInvalidDataTitle, Resources.CreateAccountEmptyField);
				return;
			}
			
			if ( ( _data.Password != _data.Confirm ) || ( _data.Password.Length < 6 || _data.Password.Length > 10 ) )
			{
				MessageHelper.Show (Resources.CreateAccountInvalidDataTitle, Resources.CreateAccountInvalidPassword);
				return;
			}
			
			LoadingOverlay.StartAnimatingLoading (this.View, LoadingOverlayPosition.Center, null, 130, 30);
			ThreadHelper.ExecuteInThread (() =>
			{
				try
				{
					string error;
					
					var result = ServiceLocator.Current.GetInstance<IAccountService> ().CreateAccount (_data, out error);
					if (result)
					{
						if ( AccountCreated != null )
						{
							AccountCreated( _data, EventArgs.Empty );
						}
						BeginInvokeOnMainThread (() => this.DismissModalViewControllerAnimated (true));
					}

					
					else
					{
						if ( error.Trim().IsNullOrEmpty() )
						{
							error = Resources.CreateAccountErrorNotSpecified;
						}
						BeginInvokeOnMainThread (() => MessageHelper.Show (Resources.CreateAccountErrorTitle, Resources.CreateAccountErrorMessage + " " + error));
					}
				}
				finally
				{
					InvokeOnMainThread (() => this.View.UserInteractionEnabled = true);
					LoadingOverlay.StopAnimatingLoading (this.View);
				}
			});
		}

		public override UIColor GetCellColor ()
		{
			return UIColor.White;
		}

		private void Cancel ()
		{
			this.DismissModalViewControllerAnimated (true);
		}

		private EntryElement CreateTextEntry (Section parentSection, string caption, string placeholder, Action<string> setValue, Func<string> getValue)
		{
			return CreateTextEntry (parentSection, caption, placeholder, setValue, getValue, false);
		}
		private EntryElement CreateTextEntry (Section parentSection, string caption, string placeholder, Action<string> setValue, Func<string> getValue, bool isPassword)
		{
			setValue( getValue() );
			var entry = new EntryElement (caption, placeholder, getValue (), isPassword);			
			entry.Changed += delegate {
				Console.WriteLine( "Changed") ;
				entry.FetchValue ();
				
				setValue (entry.Value);
			};
			parentSection.Add (entry);
			return entry;
			
		}



		private RootElement CreateListEntry (Section parentSection, string caption, Action<string> setValue, Func<string> getValue, params string[] values)
		{
			
			setValue( getValue() );
			var listSection = new Section (caption);

			int index = 0;
			foreach (string val in values)
			{
				var item = new RadioElement (val);
				item.ItemId = index;
				item.Tapped += delegate {
					setValue (values[item.ItemId]);
					this.DeactivateController(true );
					//this.
					//thithis.DismissModalViewControllerAnimated (true);
				};
				listSection.Add (item);
				index++;
			}
			
			var entry = new RootElement (caption, new RadioGroup (0));
			entry.Add (listSection);
			
			parentSection.Add (entry);
			
			
			setValue( values[0] );
			
			return entry;
			
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

		private void AddButton (UIView parent, float x, float y, string title, UIColor normal, UIColor selected, Action clicked)
		{
			var btn = UIButton.FromType (UIButtonType.RoundedRect);
			btn.Frame = new System.Drawing.RectangleF (x, y, 130, 40);
			btn.SetTitle (title, UIControlState.Normal);
			parent.AddSubview (btn);
			GlassButton.Wrap (btn, normal, selected).TouchUpInside += delegate { clicked (); };
		}
		
		private void LoadFooter()
		{
			var footer = new UIView { Frame = new RectangleF (0, 0, 320, 200) };
			this.TableView.TableFooterView = footer;
			
			AddButton (footer, 170, 25, Resources.CreateAccoutCreate, AppStyle.AcceptButtonColor, AppStyle.AcceptButtonHighlightedColor, () => CreateAccount ());
			AddButton (footer, 20, 25, Resources.CreateAccoutCancel, AppStyle.LightButtonColor, AppStyle.LightButtonHighlightedColor, () => Cancel ());
		}
		
		private void LoadSettingsElements ()
		{
			ThreadHelper.ExecuteInThread (() =>
			{
				try
				{
					if (_elementAreLoaded)
					{
						return;
					}
					_elementAreLoaded = true;
					
					var menu = new RootElement (Resources.CreateAccoutTitle);
					
					var settings = new Section (Resources.CreateAccoutTitle);
					menu.Add (settings);
					
					_emailEntry = CreateTextEntry (settings, Resources.CreateAccoutEmailLabel, null, s => _data.Email = s, () => _data.Email);
					_emailEntry.KeyboardType = UIKeyboardType.EmailAddress;
					_emailEntry.OffsetX = 50 ;
					
					_emailEntry.TextAutocapitalizationType = UITextAutocapitalizationType.None;
					_emailEntry.TextAutocorrectionType = UITextAutocorrectionType.No;
			
					
					_title = CreateListEntry (settings, Resources.CreateAccoutTitleLabel, s => _data.Title = s, () => _data.Title, Resources.CreateAccountTitleMr, Resources.CreateAccountTitleMrs);
					
					_firstNameEntry = CreateTextEntry (settings, Resources.CreateAccoutFirstNameLabel, null, s => _data.FirstName = s, () => _data.FirstName);
					_lastNameEntry = CreateTextEntry (settings, Resources.CreateAccoutLastNameLabel, null, s => _data.LastName = s, () => _data.LastName);
					_firstNameEntry.OffsetX = 30 ;
					_lastNameEntry.OffsetX = 30 ;
					
					_phoneEntry = CreateTextEntry (settings, Resources.CreateAccoutPhoneNumberLabel, null, s => _data.Phone = s, () => _data.Phone);
					_phoneEntry.KeyboardType = UIKeyboardType.PhonePad;
					
					_mobileEntry = CreateTextEntry (settings, Resources.CreateAccoutMobileLabel, null, s => _data.Mobile = s, () => _data.Mobile);
					_mobileEntry.KeyboardType = UIKeyboardType.PhonePad;
					
					//CreateListEntry (settings, Resources.CreateAccoutLanguageLabel, s => _data.Language = s, () => _data.Language, Resources.CreateAccountLanguageFrench, Resources.CreateAccountLanguageEnglish);
					_passwordEntry = CreateTextEntry (settings, Resources.CreateAccoutPasswordLabel, null, s => _data.Password = s, () => _data.Password, true);
					_passwordEntry.TextAutocapitalizationType =  UITextAutocapitalizationType.None;
					_passwordEntry.TextAutocorrectionType = UITextAutocorrectionType.No;
					
					_confirmPasswordEntry = CreateTextEntry (settings, Resources.CreateAccountPasswordConfrimation, null, s => _data.Confirm = s, () => _data.Confirm, true);					
					_confirmPasswordEntry.TextAutocapitalizationType =  UITextAutocapitalizationType.None;
					_confirmPasswordEntry.TextAutocorrectionType = UITextAutocorrectionType.No;
					
					this.InvokeOnMainThread (() => { this.Root = menu; });
					this.InvokeOnMainThread( () => LoadFooter() );
					this.InvokeOnMainThread( () => NavigationItem.Title = " " );
					
				}
				finally
				{
					
				}
				
			});

		}

	
	}
}

