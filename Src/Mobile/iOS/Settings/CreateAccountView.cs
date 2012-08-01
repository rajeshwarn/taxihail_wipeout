using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Linq;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using apcurium.Framework.Extensions;
using apcurium.Framework;
using TinyIoC;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Mobile.AppServices;

namespace apcurium.MK.Booking.Mobile.Client
{
    public class CreateAccountView : DialogViewController
    {
        private RegisterAccount _data;
        private EntryElement _emailEntry;
        private EntryElement _fullNameEntry;
        private EntryElement _phoneEntry;
        private EntryElement _passwordEntry;
        private EntryElement _confirmPasswordEntry;
        private string _confirm;
        private bool _elementAreLoaded = false;
        
        public event EventHandler AccountCreated;
        
        public CreateAccountView() : base(null)
        {
            _data = new RegisterAccount();
            _elementAreLoaded = false;
        }

        public CreateAccountView(RegisterAccount data) : base(null)
        {
            _data = data;
            _elementAreLoaded = false;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            this.InvokeOnMainThread(() => NavigationItem.Title = " ");
        }

        public override void ViewDidAppear(bool animated)
        {
            LoadSettingsElements();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
        }

        public override string GetTitle()
        {
            return "";
            //  return Resources.CreateAccoutTitle;
        }

        private void CreateAccount()
        {
            Console.WriteLine("CreateAccount");
            _emailEntry.FetchValue();
            _fullNameEntry.FetchValue();            
            _phoneEntry.FetchValue();           
            _passwordEntry.FetchValue();
            _confirmPasswordEntry.FetchValue();
            
            
            _data.Email = _emailEntry.Value;
            _data.Name = _fullNameEntry.Value;          
            _data.Phone = _phoneEntry.Value;            
            _data.Password = _passwordEntry.Value;
            string confirm = _confirmPasswordEntry.Value;
            
            if (!IsEmail(_data.Email))
            {
                MessageHelper.Show(Resources.ResetPasswordInvalidDataTitle, Resources.ResetPasswordInvalidDataMessage);
                return;
            }
            
            if (_data.Email.IsNullOrEmpty() || _data.Name.IsNullOrEmpty() || _data.Phone.IsNullOrEmpty() || _data.Password.IsNullOrEmpty() || confirm.IsNullOrEmpty())
            {
                MessageHelper.Show(Resources.CreateAccountInvalidDataTitle, Resources.CreateAccountEmptyField);
                return;
            }
            
            if ((_data.Password != confirm) || (_data.Password.Length < 6 || _data.Password.Length > 10))
            {
                MessageHelper.Show(Resources.CreateAccountInvalidDataTitle, Resources.CreateAccountInvalidPassword);
                return;
            }
            
            LoadingOverlay.StartAnimatingLoading(this.View, LoadingOverlayPosition.Center, null, 130, 30);
            ThreadHelper.ExecuteInThread(() =>
            {
                try
                {
                    string error;
                    
                    var result = TinyIoCContainer.Current.Resolve<IAccountService>().Register(_data, out error);
                    if (result)
                    {
                        if (AccountCreated != null)
                        {
                            AccountCreated(_data, EventArgs.Empty);
                        }

                        BeginInvokeOnMainThread(() => MessageHelper.Show(Resources.AccountActivationTitle, Resources.AccountActivationMessage));
                        BeginInvokeOnMainThread(() => this.DismissModalViewControllerAnimated(true));
                    }
                    else
                    {
                        if (error.Trim().IsNullOrEmpty())
                        {
                            error = Resources.CreateAccountErrorNotSpecified;
                        }
                        if (Resources.GetValue("ErrorCode_" + error) != "ErrorCode_" + error)
                        {
                            BeginInvokeOnMainThread(() => MessageHelper.Show(Resources.CreateAccountErrorTitle, Resources.CreateAccountErrorMessage + " " + Resources.GetValue("ErrorCode_" + error)));
                        }
                        else
                        {
                            BeginInvokeOnMainThread(() => MessageHelper.Show(Resources.CreateAccountErrorTitle, Resources.CreateAccountErrorMessage + " " + error));
                        }
                    }
                }
                finally
                {
                    InvokeOnMainThread(() => this.View.UserInteractionEnabled = true);
                    LoadingOverlay.StopAnimatingLoading(this.View);
                }
            }
            );
        }

        public override UIColor GetCellColor()
        {
            return UIColor.White;
        }

        private void Cancel()
        {
            this.DismissModalViewControllerAnimated(true);
        }

        private EntryElement CreateTextEntry(Section parentSection, string caption, string placeholder, Action<string> setValue, Func<string> getValue)
        {
            return CreateTextEntry(parentSection, caption, placeholder, setValue, getValue, false);
        }

        private EntryElement CreateTextEntry(Section parentSection, string caption, string placeholder, Action<string> setValue, Func<string> getValue, bool isPassword)
        {
            setValue(getValue());
            var entry = new EntryElement(caption, placeholder, getValue(), isPassword);         
            entry.Changed += delegate
            {
                Console.WriteLine("Changed");
                entry.FetchValue();
                
                setValue(entry.Value);
            };
            parentSection.Add(entry);
            return entry;
            
        }

        private RootElement CreateListEntry(Section parentSection, string caption, Action<string> setValue, Func<string> getValue, params string[] values)
        {
            
            setValue(getValue());
            var listSection = new Section(caption);

            int index = 0;
            foreach (string val in values)
            {
                var item = new RadioElement(val);
                item.ItemId = index;
                item.Tapped += delegate
                {
                    setValue(values[item.ItemId]);
                    this.DeactivateController(true);
                    //this.
                    //thithis.DismissModalViewControllerAnimated (true);
                };
                listSection.Add(item);
                index++;
            }
            
            var entry = new RootElement(caption, new RadioGroup(0));
            entry.Add(listSection);
            
            parentSection.Add(entry);
            
            
            setValue(values[0]);
            
            return entry;
            
        }

        public static bool IsEmail(string inputEmail)
        {
            inputEmail = inputEmail.ToSafeString();
            string strRegex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" + @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" + @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
            Regex re = new Regex(strRegex);
            if (re.IsMatch(inputEmail))
                return (true);
            else
                return (false);
        }
            
        private void AddButton(UIView parent, float x, float y, string title, Action clicked, AppStyle.ButtonColor bcolor)
        {
            var btn = AppButtons.CreateStandardGradientButton(new System.Drawing.RectangleF(x, y, 130, 40), title, bcolor == AppStyle.ButtonColor.Grey ? UIColor.FromRGB(101, 101, 101) : UIColor.White, bcolor);
            btn.TextShadowColor = null;

            parent.AddSubview(btn);
            btn.TouchUpInside += delegate
            {
                clicked();
            };
             
        }

//        private void AddButton(UIView parent, float x, float y, string title, UIColor normal, UIColor selected, Action clicked)
//        {
//            var btn = UIButton.FromType(UIButtonType.RoundedRect);
//            btn.Frame = new System.Drawing.RectangleF(x, y, 130, 40);
//            btn.SetTitle(title, UIControlState.Normal);
//            parent.AddSubview(btn);
//            GlassButton.Wrap(btn, normal, selected).TouchUpInside += delegate
//            {
//                clicked();
//            };
//        }
        
        private void LoadFooter()
        {
            var footer = new UIView { Frame = new RectangleF (0, 0, 320, 200) };
            this.TableView.TableFooterView = footer;
            
            AddButton(footer, 170, 25, Resources.CreateAccoutCreate, () => CreateAccount(), apcurium.MK.Booking.Mobile.Client.AppStyle.ButtonColor.Green);
            AddButton(footer, 20, 25, Resources.CreateAccoutCancel, () => Cancel(), apcurium.MK.Booking.Mobile.Client.AppStyle.ButtonColor.Grey);
        }
        
        private void LoadSettingsElements()
        {
            ThreadHelper.ExecuteInThread(() =>
            {
                try
                {
                    if (_elementAreLoaded)
                    {
                        return;
                    }
                    _elementAreLoaded = true;
                    
                    var menu = new RootElement(Resources.CreateAccoutTitle);
                    
                    var settings = new Section(Resources.CreateAccoutTitle);
                    menu.Add(settings);
                    
                    _emailEntry = CreateTextEntry(settings, Resources.CreateAccoutEmailLabel, null, s => _data.Email = s, () => _data.Email);
                    _emailEntry.KeyboardType = UIKeyboardType.EmailAddress;
                    _emailEntry.OffsetX = 50;
                    
                    _emailEntry.TextAutocapitalizationType = UITextAutocapitalizationType.None;
                    _emailEntry.TextAutocorrectionType = UITextAutocorrectionType.No;
                                
                    _fullNameEntry = CreateTextEntry(settings, Resources.CreateAccoutFullNameLabel, null, s => _data.Name = s, () => _data.Name);                   
                    _fullNameEntry.OffsetX = 30;                    
                    
                    _phoneEntry = CreateTextEntry(settings, Resources.CreateAccoutPhoneNumberLabel, null, s => _data.Phone = s, () => _data.Phone);
                    _phoneEntry.KeyboardType = UIKeyboardType.PhonePad;
                    
                    
                    _passwordEntry = CreateTextEntry(settings, Resources.CreateAccoutPasswordLabel, null, s => _data.Password = s, () => _data.Password, true);
                    _passwordEntry.TextAutocapitalizationType = UITextAutocapitalizationType.None;
                    _passwordEntry.TextAutocorrectionType = UITextAutocorrectionType.No;
                    
                    _confirmPasswordEntry = CreateTextEntry(settings, Resources.CreateAccountPasswordConfrimation, null, s => _confirm = s, () => _confirm, true);                  
                    _confirmPasswordEntry.TextAutocapitalizationType = UITextAutocapitalizationType.None;
                    _confirmPasswordEntry.TextAutocorrectionType = UITextAutocorrectionType.No;
                    
                    this.InvokeOnMainThread(() => {
                        this.Root = menu; }
                    );
                    this.InvokeOnMainThread(() => LoadFooter());
                    this.InvokeOnMainThread(() => NavigationItem.Title = " ");
                    
                }
                finally
                {
                    
                }
                
            }
            );

        }

    
    }
}

