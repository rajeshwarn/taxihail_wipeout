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


            LoadSettingsElements();
        }

        public CreateAccountView(RegisterAccount data) : base(null)
        {
            _data = data;
            _elementAreLoaded = false;
                LoadSettingsElements();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            this.InvokeOnMainThread(() => NavigationItem.Title = " ");

            View.BackgroundColor = UIColor.Clear; // UIColor.FromPatternImage(UIImage.FromFile("Assets/background.png"));
            TableView.BackgroundView = new UIView{ BackgroundColor = UIColor.Clear };
            TableView.BackgroundColor = UIColor.Clear;
			((UINavigationController ) ParentViewController ).NavigationBar.TopItem.TitleView = new TitleView(null, Resources.View_SignUp, true);
            ((UINavigationController ) ParentViewController ).View.BackgroundColor =UIColor.FromPatternImage(UIImage.FromFile("Assets/background.png"));


            //LoadBackgroundNavBar( this
        }

   

         private void LoadBackgroundNavBar(UINavigationBar bar)
        {
            bar.TintColor = AppStyle.NavigationBarColor;  

            //It might crash on iOS version smaller than 5.0
            try
            {
                bar.SetBackgroundImage(UIImage.FromFile("Assets/navBar.png"), UIBarMetrics.Default);
            }
            catch
            {
            }
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

            bool hasEmail = _data.Password.HasValue() && confirm.HasValue();
            bool hasSocialInfo = _data.FacebookId.HasValue() || _data.TwitterId.HasValue();
            if (_data.Email.IsNullOrEmpty() || _data.Name.IsNullOrEmpty() || _data.Phone.IsNullOrEmpty() || (!hasEmail && !hasSocialInfo))
            {
                MessageHelper.Show(Resources.CreateAccountInvalidDataTitle, Resources.CreateAccountEmptyField);
                return;
            }
            
            if (!hasSocialInfo && ((_data.Password != confirm) || (_data.Password.Length < 6 || _data.Password.Length > 10)))
            {
                MessageHelper.Show(Resources.CreateAccountInvalidDataTitle, Resources.CreateAccountInvalidPassword);
                return;
            }

            if ( _data.Phone.Count(x => Char.IsDigit(x)) < 10 )
            {
                MessageHelper.Show(Resources.CreateAccountInvalidDataTitle, Resources.CreateAccountInvalidPhone);
                return;
            }
            else
            {
                _data.Phone= new string( _data.Phone.ToArray().Where( c=> Char.IsDigit( c ) ).ToArray());
            }


            //var count = jobId.Count(x => Char.IsDigit(x));

            LoadingOverlay.StartAnimatingLoading( LoadingOverlayPosition.Center, null, 130, 30);
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
                        if (!hasSocialInfo)
                        {
                            BeginInvokeOnMainThread(() => MessageHelper.Show(Resources.AccountActivationTitle, Resources.AccountActivationMessage));
                        }

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
                    InvokeOnMainThread(() => 
                                       {
                        this.View.UserInteractionEnabled = true;
                    LoadingOverlay.StopAnimatingLoading();
                    });
                }
            }
            );
        }

        private void Cancel()
        {
            this.DismissModalViewControllerAnimated(true);
        }

        private EntryElement CreateTextEntry(Section parentSection, string caption, string placeholder, Action<string> setValue, Func<string> getValue)
        {
            return CreateTextEntry(parentSection, caption, placeholder, setValue, getValue, false, true);
        }

        private EntryElement CreateTextEntry(Section parentSection, string caption, string placeholder, Action<string> setValue, Func<string> getValue, bool isPassword, bool addToParentSection = true)
        {
            setValue(getValue());
            var entry = new RightAlignedEntryElement(caption, placeholder, getValue(), isPassword);         
            entry.Changed += delegate
            {
                Console.WriteLine("Changed");
                entry.FetchValue();
                
                setValue(entry.Value);
            };

            if ( addToParentSection )
            {
                parentSection.Add(entry);
            }
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
                item.Value = index.ToString();
                item.Tapped += delegate
                {
                    setValue(values[int.Parse( item.Value ) ]);
                    this.DeactivateController(true);
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
            var btn = AppButtons.CreateStandardButton(new System.Drawing.RectangleF(x, y, 130, 40), title, bcolor);
            btn.TextShadowColor = null;

            parent.AddSubview(btn);
            btn.TouchUpInside += delegate
            {
                clicked();
            };
             
        }

        
        private void LoadFooter()
        {
            var footer = new UIView { Frame = new RectangleF (0, 0, 320, 200) };
            this.TableView.TableFooterView = footer;
            
            AddButton(footer, 170, 25, Resources.CreateAccoutCreate, () => CreateAccount(), apcurium.MK.Booking.Mobile.Client.AppStyle.ButtonColor.Green);
            AddButton(footer, 20, 25, Resources.CreateAccoutCancel, () => Cancel(), apcurium.MK.Booking.Mobile.Client.AppStyle.ButtonColor.Grey);
        }

        private CustomRootElement _menu;
        private Section _settings;

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
                    
                    _menu = new CustomRootElement(Resources.GetValue("View_SignUp_Label"));
                    
                    _settings = new Section(Resources.CreateAccoutTitle);

                    _menu.Add(_settings);
                   
                    _emailEntry = CreateTextEntry(_settings, Resources.CreateAccoutEmailLabel, "", s => _data.Email = s, () => _data.Email);
                    _emailEntry.KeyboardType = UIKeyboardType.EmailAddress;
                    //_emailEntry.oOffsetX = 50;

                    _emailEntry.AutocapitalizationType = UITextAutocapitalizationType.None;
                    _emailEntry.AutocorrectionType = UITextAutocorrectionType.No;
                                
					_fullNameEntry = CreateTextEntry(_settings, Resources.CreateAccountFullName, null, s => _data.Name = s, () => _data.Name);                   
                    //_fullNameEntry.OffsetX = 30;                    
                    
                    _phoneEntry = CreateTextEntry(_settings, Resources.CreateAccoutPhoneNumberLabel, null, s => _data.Phone = s, () => _data.Phone);
                    _phoneEntry.KeyboardType = UIKeyboardType.PhonePad;


                    bool hasSocialInfo = _data.FacebookId.HasValue() || _data.TwitterId.HasValue();



                    _passwordEntry = CreateTextEntry(_settings, Resources.CreateAccoutPasswordLabel, null, s => _data.Password = s, () => _data.Password, true, !hasSocialInfo);
                    _passwordEntry.AutocapitalizationType = UITextAutocapitalizationType.None;
                    _passwordEntry.AutocorrectionType = UITextAutocorrectionType.No;
                    
                    _confirmPasswordEntry = CreateTextEntry(_settings, Resources.CreateAccountPasswordConfrimation, null, s => _confirm = s, () => _confirm, true, !hasSocialInfo);                  
                    _confirmPasswordEntry.AutocapitalizationType = UITextAutocapitalizationType.None;
                    _confirmPasswordEntry.AutocorrectionType = UITextAutocorrectionType.No;

                    this.InvokeOnMainThread(() => {
                        this.Root = _menu; }
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

