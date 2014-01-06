#if SOCIAL_NETWORKS
using SocialNetworks.Services;

#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Commands;
using TinyIoC;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Api.Contract.Resources;
using System.Threading;
using apcurium.MK.Booking.Api.Contract.Requests;
using ServiceStack.Text;
using apcurium.Framework;
using Cirrious.MvvmCross.Interfaces.Commands;
using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.Extensions;
using Cirrious.MvvmCross.Interfaces.Platform.Lifetime;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Enumeration;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private IApplicationInfoService _applicationInfoService;
        readonly IAccountService _accountService;
        readonly IPushNotificationService _pushService;
		public event EventHandler LoginSucceeded; 
		readonly IFacebookService _facebookService;
		readonly CompositeDisposable _subscriptions = new CompositeDisposable();

#if SOCIAL_NETWORKS
		readonly ITwitterService _twitterService;
		public IFacebookService FacebookService { get { return _facebookService; } }

		public LoginViewModel(IFacebookService facebookService, ITwitterService twitterService, IAccountService accountService, IApplicationInfoService applicationInfoService, IPushNotificationService pushService)
			:this(accountService, applicationInfoService, pushService)
		{
			_facebookService = facebookService;
			_twitterService = twitterService;
			_facebookService.ConnectionStatusChanged -= HandleFbConnectionStatusChanged;
			_facebookService.ConnectionStatusChanged += HandleFbConnectionStatusChanged;

			_twitterService.ConnectionStatusChanged -= HandleTwitterConnectionStatusChanged;
			_twitterService.ConnectionStatusChanged += HandleTwitterConnectionStatusChanged;
		}

#endif

        public LoginViewModel(IFacebookService facebookService, IAccountService accountService, IApplicationInfoService applicationInfoService, IPushNotificationService pushService)
        {
            _facebookService = facebookService;
            _applicationInfoService = applicationInfoService;
			_accountService = accountService;		
            _pushService = pushService;

            CheckVersion();
        }

        public override void Load()
        {
            base.Load();
#if DEBUG
            Email = "john@taxihail.com";
            Password = "password";			

#endif
        }

		public override void Start(bool firstStart)
		{
			base.Start(firstStart);

			_facebookService
				.GetAndObserveSessionStatus()
				.Where(connected => connected)
				.Subscribe(_ => OnFacebookLoginSucessful(""))
				.DisposeWith(_subscriptions);
		}

		public override void Stop()
		{
			base.Stop();
			_subscriptions.Clear();
		}

        private async void CheckVersion()
        {
            //The 2 second delay is required because the view might not be created.
            await Task.Delay(2000);
            if (AccountService.CurrentAccount != null)
            {
                TinyIoCContainer.Current.Resolve<IApplicationInfoService>().CheckVersionAsync();
            }
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
		public bool CallIsEnabled
		{
			get{

				return !Config.GetSetting("Client.HideCallDispatchButton", false);
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
						if ( CallIsEnabled )
						{
                        	var settings = TinyIoCContainer.Current.Resolve<IAppSettings> ();
                        	var companyName = settings.ApplicationName;
                        	var phoneNumber = Config.GetSetting( "DefaultPhoneNumberDisplay" );
                        	message = string.Format(Resources.GetString(e.Message), companyName, phoneNumber);
						}
						else 
						{
							message= Resources.GetString("AccountDisabled_NoCall");
						}
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
                                LoginSucess();
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

                    try
                    {
                        Thread.Sleep(1500);
                        var service = TinyIoCContainer.Current.Resolve<IAccountService>();
                        Account account;
                        if (facebookId.HasValue() || twitterId.HasValue())
                        {
                            if (facebookId.HasValue())
                            {
                                var task = service.GetFacebookAccount(facebookId);
                                task.Wait();
                                account = task.Result;
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
                            LoginSucess();
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

		#if SOCIAL_NETWORKS

		public IMvxCommand LoginTwitter
		{
			get
			{
				return new MvxRelayCommand(() =>
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

		private void CheckFacebookAccount()
        {
            MessageService.ShowProgress(true);

            _facebookService.GetUserInfos(info =>
            {
                var data = new RegisterAccount();
                data.FacebookId = info.Id;
                data.Email = info.Email;
                data.Name = Params.Get(info.Firstname, info.Lastname).Where(n => n.HasValue()).JoinBy(" ");

                try
                {
                    var account = _accountService.GetFacebookAccount(data.FacebookId);
                    if (account == null)
                    {
                        DoSignUp(data);
                    }
                    else
                    {                                
                        Task.Factory.SafeStartNew(() =>
                                                  {
                            try
                            {
                                LoginSucess();
                            }
                            finally
                            {
                            }
                        });
                    }
                }
                finally
                {
                    MessageService.ShowProgress(false);
                }

            }, () => MessageService.ShowProgress(false) );


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
                        Task.Factory.SafeStartNew(() =>
                                                  {
                            try
                            {
                                LoginSucess();
                            }
                            finally
                            {
                            }
                        });
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

		void HandleFbConnectionStatusChanged(object sender, SocialNetworks.Services.Entities.FacebookStatus e)
		{
			if (e.IsConnected)
			{
				CheckFacebookAccount();
			}
		}
#endif

        public IMvxCommand LoginFacebook
        {
            get
            {
                return base
                        .GetCommand(
                            () => _facebookService.Connect("email"),
                            () => true);
            }
        }

        public void SetServerUrl(string serverUrl)
        {
            TinyIoCContainer.Current.Resolve<IAppSettings>().ServiceUrl = serverUrl;
            TinyIoCContainer.Current.Resolve<IApplicationInfoService>().ClearAppInfo();
            TinyIoCContainer.Current.Resolve<IAccountService>().ClearReferenceData();
            TinyIoCContainer.Current.Resolve<IConfigurationManager>().Reset();
        }

        private void LoginSucess()
        {
#if SOCIAL_NETWORKS
            _facebookService.ConnectionStatusChanged -= HandleFbConnectionStatusChanged;
            _twitterService.ConnectionStatusChanged -= HandleTwitterConnectionStatusChanged;
#endif

			_applicationInfoService = null;     

            RequestNavigate<BookViewModel>(true);
			if (LoginSucceeded != null)
			{
				LoginSucceeded(this, EventArgs.Empty);
			}
        }

		public void OnFacebookLoginSucessful(string accessToken)
		{
			CheckFacebookAccount(accessToken);
		}

		private async void CheckFacebookAccount(string accessToken)
		{
			using (MessageService.ShowProgress())
			{
				var info = await _facebookService.GetUserInfo(accessToken);

				var data = new RegisterAccount();
				data.FacebookId = info.Id;
				data.Email = info.Email;
				data.Name = Params.Get(info.Firstname, info.Lastname).Where(n => n.HasValue()).JoinBy(" ");

				var account = await _accountService.GetFacebookAccount(data.FacebookId);
				if (account == null)
				{
					DoSignUp(data);
				}
				else
				{
					LoginSucess();
				}
			}

		}

    }
}
