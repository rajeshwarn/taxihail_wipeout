using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ServiceStack.Text;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Framework;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Booking.Mobile.AppServices.Social;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
		public event EventHandler LoginSucceeded; 
        readonly IPushNotificationService _pushService;
		readonly IFacebookService _facebookService;
		readonly ITwitterService _twitterService;

        public LoginViewModel(IFacebookService facebookService,
			ITwitterService twitterService,
			IPushNotificationService pushService)
        {
            _facebookService = facebookService;
            _pushService = pushService;
			_twitterService = twitterService;
			_twitterService.ConnectionStatusChanged += HandleTwitterConnectionStatusChanged;

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

        private async void CheckVersion()
        {
            //The 2 second delay is required because the view might not be created.
            await Task.Delay(2000);
            if (this.Services().Account.CurrentAccount != null)
            {
                this.Services().ApplicationInfo.CheckVersionAsync();
            }
        }


        private string _email;
        public string Email
        {
            get { return _email; }
            set
            {
                _email = value;
				RaisePropertyChanged();
            }
        }

        private string _password;
        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
				RaisePropertyChanged();
            }
        }

        public AsyncCommand SignInCommand
        {
            get
            {
                return GetCommand(() =>
                {
                    this.Services().Account.ClearCache();
                    SignIn();
                });
            }
        }
		public bool CallIsEnabled
		{
			get{

                return !this.Services().Config.GetSetting("Client.HideCallDispatchButton", false);
			}

		}
        private void SignIn()
        {
            bool needToHideProgress = true;
            try
            {
                Logger.LogMessage("SignIn with server {0}", this.Services().Settings.ServiceUrl);
                this.Services().Message.ShowProgress(true);
                var account = default(Account);
                try
                {
                    account = this.Services().Account.GetAccount(Email, Password);                 
                }
                catch (Exception e)
                {
                    var title = this.Services().Localize["InvalidLoginMessageTitle"];
                    var message = this.Services().Localize[e.Message];

                    if(e.Message == AuthenticationErrorCode.AccountDisabled){
						if ( CallIsEnabled )
						{
                            var companyName = this.Services().Settings.ApplicationName;
                            var phoneNumber = this.Services().Config.GetSetting("DefaultPhoneNumberDisplay");
                            message = string.Format(this.Services().Localize[e.Message], companyName, phoneNumber);
						}
						else 
						{
                            message = this.Services().Localize["AccountDisabled_NoCall"];
						}
                    }
                    this.Services().Message.ShowMessage(title, message);
                }

                if (account != null)
                {
                    needToHideProgress = false;
                    Password = string.Empty;
                

                    InvokeOnMainThread(()=> _pushService.RegisterDeviceForPushNotifications(force: true));

                    Task.Factory.SafeStartNew(() =>
                        {
                            try
                            {
                                OnLoginSuccess();
                            }
                            finally
                            {
                                Thread.Sleep(1000);
								InvokeOnMainThread(() => this.Services().Message.ShowProgress(false));
                            }
                        });

                }
            }
            finally
            {
                if (needToHideProgress)
                {
                    this.Services().Message.ShowProgress(false);
                }
            }
        }

        public AsyncCommand ResetPassword
        {
            get
            {
				return GetCommand(() => ShowSubViewModel<ResetPasswordViewModel, string>(null, email => {
                                                                                                              if(email.HasValue())
                                                                                                              {
                                                                                                                  Email = email;
                                                                                                              }
                }));

            }
        }

        public AsyncCommand SignUp
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
			ShowSubViewModel<CreateAcccountViewModel, RegisterAccount>(new Dictionary<string, string> { { "data", serialized } }, OnAccountCreated);
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
                        Account account;
                        if (facebookId.HasValue() || twitterId.HasValue())
                        {
                            if (facebookId.HasValue())
                            {
                                var task = this.Services().Account.GetFacebookAccount(facebookId);
                                task.Wait();
                                account = task.Result;
                            }
                            else
                            {
                                account = this.Services().Account.GetTwitterAccount(twitterId);
                            }
                        }
                        else
                        {
                            account = this.Services().Account.GetAccount(data.Email, data.Password);
                        }

                        if (account != null)
                        {
                            OnLoginSuccess();
                        }
                    }
                    catch(Exception ex)
                    {
                        Logger.LogError(ex);
                    }
                    finally
                    {
                        this.Services().Message.ShowProgress(false);
                    }
                }
                else
                {
                    Email = data.Email;
                }
            }
        }

		public ICommand LoginTwitter
		{
			get
			{
				return base.GetCommand(() =>
				{
					if (_twitterService.IsConnected)
					{
						CheckTwitterAccount();
					}
					else
					{
						_twitterService.Connect();
					}
					}, () => true);
			}
		}

        private void CheckTwitterAccount()
        {
			this.Services().Message.ShowProgress(true);

            _twitterService.GetUserInfos(info =>
            {
                var data = new RegisterAccount();
                data.TwitterId = info.Id;
                data.Name = Params.Get(info.Firstname, info.Lastname).Where(n => n.HasValue()).JoinBy(" ");

                try
                {
						var account = this.Services().Account.GetTwitterAccount(data.TwitterId);
                    if (account == null)
                    {
                        DoSignUp(data);
                    }
                    else
                    {
						Task.Factory.SafeStartNew(() => OnLoginSuccess());
                    }
                }
                finally
                {
						this.Services().Message.ShowProgress(false);
                }
            });
        }


        void HandleTwitterConnectionStatusChanged(object sender, TwitterStatus e)
        {
            if (e.IsConnected)
            {
                CheckTwitterAccount();
            }
        }

        public ICommand LoginFacebook
        {
            get
            {
                return base
                        .GetCommand(
							async () => {
								try
								{
									await _facebookService.Connect();
									CheckFacebookAccount();
								}
								catch(TaskCanceledException)
								{
									Logger.LogMessage("FacebookService.Connect was cancelled");
								}
								catch(Exception e)
								{
									Logger.LogError(e);
								}
							},
                            () => true);
            }
        }

        public void SetServerUrl(string serverUrl)
        {
            this.Services().Settings.ServiceUrl = serverUrl;
            this.Services().ApplicationInfo.ClearAppInfo();
            this.Services().Account.ClearReferenceData();
            this.Services().Config.Reset();
        }

        private void OnLoginSuccess()
        {
            _twitterService.ConnectionStatusChanged -= HandleTwitterConnectionStatusChanged;

			ShowViewModel<BookViewModel>(true);
			if (LoginSucceeded != null)
			{
				LoginSucceeded(this, EventArgs.Empty);
			}
        }

		private async void CheckFacebookAccount()
		{
			using (this.Services().Message.ShowProgress())
			{
				var info = await _facebookService.GetUserInfo();

				var data = new RegisterAccount();
				data.FacebookId = info.Id;
				data.Email = info.Email;
				data.Name = Params.Get(info.Firstname, info.Lastname)
					.Where(n => n.HasValue()).JoinBy(" ");

				var account = await this.Services().Account.GetFacebookAccount(data.FacebookId);
				if (account == null)
				{
					DoSignUp(data);
				}
				else
				{
					OnLoginSuccess();
				}
			}

		}

    }
}
