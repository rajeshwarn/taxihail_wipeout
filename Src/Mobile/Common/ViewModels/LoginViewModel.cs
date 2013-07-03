using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Commands;
using MK.Common.Android.Configuration;
using TinyIoC;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Api.Contract.Resources;
using System.Threading;
using apcurium.MK.Booking.Api.Contract.Requests;
using ServiceStack.Text;
using SocialNetworks.Services;
using apcurium.Framework;
using Cirrious.MvvmCross.Interfaces.Commands;
using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.Extensions;
using Cirrious.MvvmCross.Interfaces.Platform.Lifetime;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private IApplicationInfoService _applicationInfoService;
        readonly IAccountService _accountService;
        readonly IFacebookService _facebookService;
        readonly ITwitterService _twitterService;
        readonly IPushNotificationService _pushService;

        public LoginViewModel(IFacebookService facebookService, ITwitterService twitterService, IAccountService accountService, IApplicationInfoService applicationInfoService, IPushNotificationService pushService)
        {
            _applicationInfoService = applicationInfoService;
			_accountService = accountService;		
            _pushService = pushService;

            CheckVersion();
            _facebookService = facebookService;
            _twitterService = twitterService;
            _facebookService.ConnectionStatusChanged -= HandleFbConnectionStatusChanged;
            _facebookService.ConnectionStatusChanged += HandleFbConnectionStatusChanged;

            _twitterService.ConnectionStatusChanged -= HandleTwitterConnectionStatusChanged;
            _twitterService.ConnectionStatusChanged += HandleTwitterConnectionStatusChanged;
            
        }

   
        public override void Load()
        {
            base.Load();
#if DEBUG
            Email = "john@taxihail.com";
            Password = "password";			
#endif
        }

        private void CheckVersion()
        {
            Task.Factory.SafeStartNew(() =>
            {
                
                //The 2 second delay is required because the view might not be created.                
                Thread.Sleep(2000);
                if (_accountService.CurrentAccount != null)
                {
                    TinyIoCContainer.Current.Resolve<IApplicationInfoService>().CheckVersion();
                }
            });

        }


        private string _email;
        public string Email
        {
            get { return _email; }
            set
            {
                _email = value;
                FirePropertyChanged(() => Email);
            }
        }

        private string _password;
        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                FirePropertyChanged(() => Password);
            }
        }

        public IMvxCommand SignInCommand
        {
            get
            {
                return GetCommand(() =>
                {
                    _accountService.ClearCache();
                    SignIn();
                });
            }
        }

        private void SignIn()
        {
            bool needToHideProgress = true;
            try
            {
                Logger.LogMessage("SignIn with server {0}", Settings.ServiceUrl);
                MessageService.ShowProgress(true);
                var account = default(Account);
                
                try
                {
                    account = _accountService.GetAccount(Email, Password);
                }
                catch (Exception e)
                {
                    var title = Resources.GetString("InvalidLoginMessageTitle");
                    var message = Resources.GetString(e.Message);
                    if(e.Message == AuthenticationErrorCode.AccountDisabled){
                        var settings = TinyIoCContainer.Current.Resolve<IAppSettings> ();
                        var companyName = settings.ApplicationName;
                        var phoneNumber = settings.PhoneNumberDisplay(0);
                        message = string.Format(Resources.GetString(e.Message), companyName, phoneNumber);
                    }

                    MessageService.ShowMessage(title, message);
                }

                if (account != null)
                {
                    needToHideProgress = false;
                    this.Password = string.Empty;

                    InvokeOnMainThread(()=> _pushService.RegisterDeviceForPushNotifications(force: true));

                    Task.Factory.SafeStartNew(() =>
                        {
                            try
                            {
                                RequestNavigate<BookViewModel>(true);
                                RequestMainThreadAction( () => RequestClose(this));
                            }
                            finally
                            {
                                Thread.Sleep(1000);
                                RequestMainThreadAction( () => MessageService.ShowProgress(false) );
                            }
                        });

                }
            }
            finally
            {
                if (needToHideProgress)
                {
                    MessageService.ShowProgress(false);
                }
            }
        }

        public IMvxCommand ResetPassword
        {
            get
            {
                return GetCommand(() => RequestSubNavigate<ResetPasswordViewModel, string>(null, email => {
                                                                                                              if(email.HasValue())
                                                                                                              {
                                                                                                                  Email = email;
                                                                                                              }
                }));

            }
        }

        public IMvxCommand SignUp
        {
            get
            {
                return GetCommand(() => DoSignUp());
            }
        }

        private void DoSignUp(RegisterAccount registerDataFromSocial = null)
        {
            string serialized = null;
            if (registerDataFromSocial != null)
            {
                serialized = registerDataFromSocial.ToJson();
            }
            RequestSubNavigate<CreateAcccountViewModel, RegisterAccount>(new Dictionary<string, string> { { "data", serialized } }, OnAccountCreated);
        }

        void OnAccountCreated(RegisterAccount data)
        {
            if (data != null)
            {
                if (data.FacebookId.HasValue() || data.TwitterId.HasValue() || data.AccountActivationDisabled)
                {
                    var facebookId = data.FacebookId;
                    var twitterId = data.TwitterId;
                    MessageService.ShowProgress(true);
                    try
                    {
                        Thread.Sleep(500);
                        var service = TinyIoCContainer.Current.Resolve<IAccountService>();
                        Account account;
                        if (facebookId.HasValue() || twitterId.HasValue())
                        {
                            if (facebookId.HasValue())
                            {
                                account = service.GetFacebookAccount(facebookId);
                            }
                            else
                            {
                                account = service.GetTwitterAccount(twitterId);
                            }
                        }
                        else
                        {
                            account = service.GetAccount(data.Email, data.Password);
                        }

                        if (account != null)
                        {
                            RequestNavigate<BookViewModel>(true);
                        }
                    }
                    catch
                    {
                    }
                    finally
                    {
                        MessageService.ShowProgress(false);
                    }
                }
                else
                {
                    Email = data.Email;
                }
            }
        }

        public IMvxCommand LoginFacebook
        {
            get
            {
                return GetCommand(() =>
                {
                    if (_facebookService.IsConnected)
                    {
                        CheckFacebookAccount();
                    }
                    else
                    {
                        _facebookService.Connect("email, publish_stream, publish_actions");

                    }
                });
            }
        }

        public IMvxCommand LoginTwitter
        {
            get
            {
                return GetCommand(() =>
                {
                    if (_twitterService.IsConnected)
                    {
                        CheckTwitterAccount();
                    }
                    else
                    {
                        _twitterService.Connect();

                    }
                });
            }
        }

        void HandleFbConnectionStatusChanged(object sender, SocialNetworks.Services.Entities.FacebookStatus e)
        {
            if (e.IsConnected)
            {
                CheckFacebookAccount();
            }
        }

        private void CheckFacebookAccount()
        {
            MessageService.ShowProgress(true);


//            _facebookService.GetUserInfos(info =>
//            {
//                var data = new RegisterAccount();
//                data.FacebookId = info.Id;
//                data.Email = info.Email;
//                data.Name = Params.Get(info.Firstname, info.Lastname).Where(n => n.HasValue()).JoinBy(" ");
//
//                try
//                {
//                    var account = _accountService.GetFacebookAccount(data.FacebookId);
//                    if (account == null)
//                    {
//                        DoSignUp(data);
//                    }
//                    else
//                    {
//                        RequestNavigate<BookViewModel>(true);
//                    }
//                }
//                finally
//                {
//                    MessageService.ShowProgress(false);
//                }
//
//            }, Nothing);
        }

        private void Nothing()
        {
             }
        private void CheckTwitterAccount()
        {
            MessageService.ShowProgress(true);

            _twitterService.GetUserInfos(info =>
            {
                var data = new RegisterAccount();
                data.TwitterId = info.Id;
                data.Name = Params.Get(info.Firstname, info.Lastname).Where(n => n.HasValue()).JoinBy(" ");

                try
                {
                    var account = _accountService.GetTwitterAccount(data.TwitterId);
                    if (account == null)
                    {
                        DoSignUp(data);
                    }
                    else
                    {
                        RequestNavigate<BookViewModel>(true);
                    }
                }
                finally
                {
                    MessageService.ShowProgress(false);
                }
            }
            );
        }

        void HandleTwitterConnectionStatusChanged(object sender, SocialNetworks.Services.Entities.TwitterStatus e)
        {
            if (e.IsConnected)
            {
                CheckTwitterAccount();
            }
        }

        public void SetServerUrl(string serverUrl)
        {
            TinyIoCContainer.Current.Resolve<IAppSettings>().ServiceUrl = serverUrl;
            TinyIoCContainer.Current.Resolve<IApplicationInfoService>().ClearAppInfo();
            TinyIoCContainer.Current.Resolve<IAccountService>().ClearReferenceData();
            TinyIoCContainer.Current.Resolve<IConfigurationManager>().Reset();
        }
    }
}
