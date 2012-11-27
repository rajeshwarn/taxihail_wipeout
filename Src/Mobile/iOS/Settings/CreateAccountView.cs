using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Linq;
using MonoTouch.UIKit;
using apcurium.Framework.Extensions;
using apcurium.Framework;
using TinyIoC;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Mobile.AppServices;
using Cirrious.MvvmCross.Dialog.Touch;
using Cirrious.MvvmCross.Dialog.Touch.Dialog.Elements;
using Cirrious.MvvmCross.Views;
using Cirrious.MvvmCross.Binding.Touch.ExtensionMethods;

namespace apcurium.MK.Booking.Mobile.Client
{
	public class CreateAccountView : MvxTouchDialogViewController<CreateAcccountViewModel>
    {
        
        private bool _elementAreLoaded = false;
        
        public CreateAccountView(MvxShowViewModelRequest request)
			: base(request, UITableViewStyle.Grouped, null, false)
		{
			_elementAreLoaded = false;
		}
		
		public CreateAccountView(MvxShowViewModelRequest request, UITableViewStyle style, RootElement root, bool pushing)
			: base(request, style, root, pushing)
		{
			_elementAreLoaded = false;
		}      

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			LoadSettingsElements ();     
		}
        

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            View.BackgroundColor = UIColor.Clear;
            TableView.BackgroundView = new UIView{ BackgroundColor = UIColor.Clear };
            TableView.BackgroundColor = UIColor.Clear;
            NavigationItem.HidesBackButton = true;
        }

        private EntryElement CreateTextEntry(Section parentSection, string caption, string placeholder)
        {
            return CreateTextEntry(parentSection, caption, placeholder, false, true);
        }

        private EntryElement CreateTextEntry(Section parentSection, string caption, string placeholder, bool isPassword, bool addToParentSection = true)
        {            
			var entry = new RightAlignedMvvmCrossEntryElement(caption, placeholder, null, isPassword);  
			if ( addToParentSection )
            {
                parentSection.Add(entry);
            }
            return entry;
            
        }            
            
        private void AddButton(UIView parent, float x, float y, string title, string command, AppStyle.ButtonColor bcolor)
        {
            var btn = AppButtons.CreateStandardButton(new System.Drawing.RectangleF(x, y, 130, 40), title, bcolor);
            btn.TextShadowColor = null;
            parent.AddSubview(btn);
			this.AddBindings(btn, "{'TouchUpInside': {'Path' : '" + command + "'}}");              
        }
        
        private void LoadFooter()
        {
            var footer = new UIView { Frame = new RectangleF (0, 0, 320, 200) };
            this.TableView.TableFooterView = footer;
            
			AddButton(footer, 170, 25, Resources.CreateAccoutCreate, "CreateAccount", apcurium.MK.Booking.Mobile.Client.AppStyle.ButtonColor.Green);
			AddButton(footer, 20, 25, Resources.CreateAccoutCancel, "Cancel", apcurium.MK.Booking.Mobile.Client.AppStyle.ButtonColor.Grey);
        }		

        private void LoadSettingsElements()
        {
            ThreadHelper.ExecuteInThread(() =>
            {                
                if (_elementAreLoaded)
                {
                    return;
                }
                _elementAreLoaded = true;
                
                var _menu = new RootElement(Resources.View_SignUp);
                
                var _settings = new Section(Resources.GetValue("View_SignUp_Label"));

                _menu.Add(_settings);
               
                var _emailEntry = CreateTextEntry(_settings, Resources.CreateAccoutEmailLabel, "");
                _emailEntry.KeyboardType = UIKeyboardType.EmailAddress;
                _emailEntry.AutocapitalizationType = UITextAutocapitalizationType.None;
                _emailEntry.AutocorrectionType = UITextAutocorrectionType.No;
				_emailEntry.Bind (this, "{'Value':{'Path':'Data.Email','Mode':'TwoWay'}}");
                            
                var _fullNameEntry = CreateTextEntry(_settings, Resources.CreateAccountFullName, null); 
				_fullNameEntry.Bind (this, "{'Value':{'Path':'Data.Name','Mode':'TwoWay'}}");
                
                var _phoneEntry = CreateTextEntry(_settings, Resources.CreateAccoutPhoneNumberLabel, null);
                _phoneEntry.KeyboardType = UIKeyboardType.PhonePad;
				_phoneEntry.Bind (this, "{'Value':{'Path':'Data.Phone','Mode':'TwoWay'}}");

                 var _passwordEntry = CreateTextEntry(_settings, Resources.CreateAccoutPasswordLabel, null, true, !ViewModel.HasSocialInfo);
                _passwordEntry.AutocapitalizationType = UITextAutocapitalizationType.None;
                _passwordEntry.AutocorrectionType = UITextAutocorrectionType.No;
				_passwordEntry.Bind (this, "{'Value':{'Path':'Data.Password','Mode':'TwoWay'}}");
                
                var _confirmPasswordEntry = CreateTextEntry(_settings, Resources.CreateAccountPasswordConfrimation, null, true, !ViewModel.HasSocialInfo);                  
                _confirmPasswordEntry.AutocapitalizationType = UITextAutocapitalizationType.None;
                _confirmPasswordEntry.AutocorrectionType = UITextAutocorrectionType.No;
				_confirmPasswordEntry.Bind (this, "{'Value':{'Path':'ConfirmPassword','Mode':'TwoWay'}}");
                this.InvokeOnMainThread(() => { this.Root = _menu; });  
                this.InvokeOnMainThread(() => LoadFooter());

            }
            );

        }

    
    }
}

