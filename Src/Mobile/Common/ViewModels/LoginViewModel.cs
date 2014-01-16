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
using apcurium.MK.Booking.Mobile.AppServices;

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

        }

        public override void Start()
        {
#if DEBUG
            Email = "john@taxihail.com";
            Password = "password";          
#endif
        }

        public override void Start(bool firstStart)
        {
            base.Start(firstStart);

            CheckVersion();
        }

        private async void CheckVersion()
        {
            await Task.Delay(1000);
            this.Services().ApplicationInfo.CheckVersionAsync();
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
				return GetCommand(async () =>
                {
                    this.Services().Account.ClearCache();
					await SignIn();
                });
            }
        }
		public bool CallIsEnabled
		{
			get{

                return !this.Services().Config.GetSetting("Client.HideCallDispatchButton", false);
			}

		}

        private async Task SignIn()
        {
            using(this.Services().Message.ShowProgress())
            {
                try
                {
                    await this.Services().Account.SignIn(Email, Password);   
                    Password = string.Empty;
                    OnLoginSuccess();
                                 
                }
                catch (AuthException e)
                {
                    var localize = this.Services().Localize;
                    if(e.Failure == AuthFailure.NetworkError || e.Failure == AuthFailure.InvalidServiceUrl)
                    {
                        var title = localize["NoConnectionTitle"];
                        var msg = localize["NoConnectionMessage"];
                        this.Services().Message.ShowMessage (title, msg);
                    }
                    else if(e.Failure == AuthFailure.InvalidUsernameOrPassword)
                    {
                        var title = localize["InvalidLoginMessageTitle"];
                        var message = localize["InvalidLoginMessage"];
                        this.Services().Message.ShowMessage (title, message);
                    }
                    else if(e.Failure == AuthFailure.AccountDisabled)
                    {
                        var title = this.Services().Localize["InvalidLoginMessageTitle"];
                        string message = null;
                        if (CallIsEnabled)
                        {
                            var companyName = this.Services().Settings.ApplicationName;
                            var phoneNumber = this.Services().Config.GetSetting("DefaultPhoneNumberDisplay");
                            message = string.Format(localize[e.Message], companyName, phoneNumber);
                        }
                        else 
                        {
                            message = localize["AccountDisabled_NoCall"];
                        }
                        this.Services().Message.ShowMessage(title, message);
                    }
                }
                catch (Exception e)
                {
                    var localize = this.Services().Localize;
                    var title = localize["InvalidLoginMessageTitle"];
                    var message = localize[e.Message];

                    this.Services().Message.ShowMessage(title, message);
                }

                InvokeOnMainThread(()=> _pushService.RegisterDeviceForPushNotifications(force: true));
            }
        }

        public AsyncCommand ResetPassword
        {
            get
            {
                return GetCommand(() => ShowSubViewModel<ResetPasswordViewModel, string>(null, email =>
                {
                    if (email.HasValue())
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
			ShowSubViewModel<CreateAccountViewModel, RegisterAccount>(new Dictionary<string, string> { { "data", serialized } }, OnAccountCreated);
        }

        async void OnAccountCreated(RegisterAccount data)
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
                            account = await this.Services().Account.SignIn(data.Email, data.Password);
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
