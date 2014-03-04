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
using apcurium.MK.Common.Configuration;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
		public event EventHandler LoginSucceeded; 
		private readonly IFacebookService _facebookService;
		private readonly ITwitterService _twitterService;
        private bool _loginWasSuccesful = false;

        public LoginViewModel(IFacebookService facebookService,
			ITwitterService twitterService)
        {
            _facebookService = facebookService;
			_twitterService = twitterService;
			_twitterService.ConnectionStatusChanged += HandleTwitterConnectionStatusChanged;
        }

        public override void Start()
        {
#if DEBUG
            Email = "matthieu@live.com";
            Password = "password";          
#endif
        }

        public override void OnViewStarted(bool firstTime)
        {
            base.OnViewStarted(firstTime);

            this.Services().Location.Start();

            CheckVersion();
        }

        public override void OnViewStopped()
        {
            base.OnViewStopped();

            if (!_loginWasSuccesful)
            {
                this.Services().Location.Stop();
            }
        }
        private void CheckVersion()
        {
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

        public bool CallIsEnabled
        {
            get
            {
				return !Settings.HideCallDispatchButton;
            }

        }

        public ICommand SignInCommand
        {
            get
            {
				return this.GetCommand(async () =>
                {
                    this.Services().Account.ClearCache();
					await SignIn();
                });
            }
        }

        public ICommand SignUp
        {
            get
            {
                return this.GetCommand(() => DoSignUp());
            }
        }

        public ICommand ResetPassword
        {
            get
            {
                return this.GetCommand(() => ShowSubViewModel<ResetPasswordViewModel, string>(null, email =>
                    {
                        if (email.HasValue())
                        {
                            Email = email;
                        }
                    }));
            }
        }

        public ICommand LoginTwitter
        {
            get
            {
                return this.GetCommand(() =>
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

        public ICommand LoginFacebook
        {
            get
            {
                return this.GetCommand(async () =>
                {
                    try
                    {
                        await _facebookService.Connect();
                        await CheckFacebookAccount();
                    }
                    catch (TaskCanceledException)
                    {
                        Logger.LogMessage("FacebookService.Connect was cancelled");
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(e);
                    }
                }, () => true);
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
                    if(e.Failure == AuthFailure.NetworkError 
                        || e.Failure == AuthFailure.InvalidServiceUrl)
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
							var companyName = Settings.ApplicationName;
							var phoneNumber = Settings.DefaultPhoneNumberDisplay;
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

            }
        }

        private void DoSignUp(object registerDataFromSocial = null)
        {
			ShowSubViewModel<CreateAccountViewModel, RegisterAccount>(registerDataFromSocial, OnAccountCreated);
        }

        private async void OnAccountCreated(RegisterAccount data)
        {
            if (data.FacebookId.HasValue() || data.TwitterId.HasValue() || data.AccountActivationDisabled)
            {
                var facebookId = data.FacebookId;
                var twitterId = data.TwitterId;

                using (this.Services().Message.ShowProgress())
                {
                    try
                    {
                        if (facebookId.HasValue())
                        {
                            await this.Services().Account.GetFacebookAccount(facebookId);
                        }
                        else if (twitterId.HasValue())
                        {
                            await this.Services().Account.GetTwitterAccount(twitterId);
                        }
                        else
                        {
                            await this.Services().Account.SignIn(data.Email, data.Password);
                        }

						OnLoginSuccess();
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex);
                    }
                }
            }
            else
            {
                Email = data.Email;
            }
        }

        private void HandleTwitterConnectionStatusChanged(object sender, TwitterStatus e)
        {
            if (e.IsConnected)
            {
                CheckTwitterAccount();
            }
        }

        public void SetServerUrl(string serverUrl)
        {
			this.Container.Resolve<IAppSettings>().ChangeServerUrl(serverUrl);
            this.Services().ApplicationInfo.ClearAppInfo();
            this.Services().Account.ClearReferenceData();
        }

		private void OnLoginSuccess()
        {
            _loginWasSuccesful = true;
            _twitterService.ConnectionStatusChanged -= HandleTwitterConnectionStatusChanged;

			if (NeedsToNavigateToAddCreditCard())
			{
				ShowViewModelAndRemoveFromHistory<CreditCardAddViewModel>(new { showInstructions =  true });
				return;
			}

			ShowViewModelAndRemoveFromHistory<HomeViewModel>(new { locateUser =  true });
			if (LoginSucceeded != null)
			{
				LoginSucceeded(this, EventArgs.Empty);
			}
        }

		private bool NeedsToNavigateToAddCreditCard()
		{
			if (this.Settings.CreditCardIsMandatory)
			{
				if (!this.Services().Account.CurrentAccount.DefaultCreditCard.HasValue)
				{
					return true;
				}
			}
			return false;
		}

        private async Task CheckFacebookAccount()
		{
			using (this.Services().Message.ShowProgress())
			{
				var info = await _facebookService.GetUserInfo();

				var data = new RegisterAccount();
				data.FacebookId = info.Id;
				data.Email = info.Email;
				data.Name = Params.Get(info.Firstname, info.Lastname)
					.Where(n => n.HasValue()).JoinBy(" ");

                try
                {
                    await this.Services().Account.GetFacebookAccount(info.Id);
					OnLoginSuccess();
                }
                catch(Exception)
                {
                    DoSignUp(new
                        {
                            facebookId = info.Id,
                            name = Params.Get(info.Firstname, info.Lastname).Where(n => n.HasValue()).JoinBy(" "),
                            email = info.Email,
                        });
                }
			}

		}

        private void CheckTwitterAccount()
        {
			this.Services().Message.ShowProgress(true);

            _twitterService.GetUserInfos(async info =>
                {
                    try
                    {
                        await this.Services().Account.GetTwitterAccount(info.Id);
						OnLoginSuccess();
                    }
                    catch(Exception)
                    {
                        DoSignUp(new
                            {
                                twitterId = info.Id,
                                name = Params.Get(info.Firstname, info.Lastname).Where(n => n.HasValue()).JoinBy(" "),
                            });
                    }
                    finally
                    {
					this.Services().Message.ShowProgress(false);
                    }
                });
        }

    }
}
