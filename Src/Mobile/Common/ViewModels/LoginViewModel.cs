using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.AppServices.Social;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Framework;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class LoginViewModel : PageViewModel
    {
		private readonly IFacebookService _facebookService;
		private readonly ITwitterService _twitterService;
		private readonly ILocationService _locationService;
		private readonly IAccountService _accountService;

        public LoginViewModel(IFacebookService facebookService,
			ITwitterService twitterService,
			ILocationService locationService,
			IAccountService accountService)
        {
            _facebookService = facebookService;
			_twitterService = twitterService;
			_twitterService.ConnectionStatusChanged += HandleTwitterConnectionStatusChanged;
			_locationService = locationService;
			_accountService = accountService;
        }

		public event EventHandler LoginSucceeded; 
		private bool _loginWasSuccesful = false;

        public override void Start()
        {
#if DEBUG
			//Email = "john@taxihail.com";
			//Password = "password";          
#endif
        }

        public override void OnViewStarted(bool firstTime)
        {
            base.OnViewStarted(firstTime);

			_locationService.Start();

            CheckVersion();
        }

        public override void OnViewStopped()
        {
            base.OnViewStopped();

            if (!_loginWasSuccesful)
            {
				_locationService.Stop();
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
				_signInCommand.RaiseCanExecuteChanged ();
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
				_signInCommand.RaiseCanExecuteChanged ();
            }
        }

		private AsyncCommand _signInCommand;
		public ICommand SignInCommand
        {
            get
            {
				if (_signInCommand == null) {

					_signInCommand = (AsyncCommand)this.GetCommand(async () =>
						{
							_accountService.ClearCache();
							await SignIn();
						}, CanSignIn);
				}
				return _signInCommand;
            }
        }

		private bool CanSignIn()
		{
			return Email.HasValue ()
			&& Password.HasValue ();
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
					await _accountService.SignIn(Email, Password);   
                    Password = string.Empty;                    
					OnLoginSuccess();
                }
                catch (AuthException e)
                {
                    var localize = this.Services().Localize;
                    switch (e.Failure)
                    {
                        case AuthFailure.InvalidServiceUrl:
                        case AuthFailure.NetworkError:
                        {
                            var title = localize["NoConnectionTitle"];
                            var msg = localize["NoConnectionMessage"];
                            this.Services().Message.ShowMessage (title, msg);
                        }
                            break;
                        case AuthFailure.InvalidUsernameOrPassword:
                        {
                            var title = localize["InvalidLoginMessageTitle"];
                            var message = localize["InvalidLoginMessage"];
                            this.Services().Message.ShowMessage (title, message);
                        }
                            break;
                        case AuthFailure.AccountDisabled:
                        {
                            var title = this.Services().Localize["InvalidLoginMessageTitle"];
                            string message = null;
                            if (!Settings.HideCallDispatchButton)
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
                            break;
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
							await _accountService.GetFacebookAccount(facebookId);
                        }
                        else if (twitterId.HasValue())
                        {
							await _accountService.GetTwitterAccount(twitterId);
                        }
                        else
                        {
							await _accountService.SignIn(data.Email, data.Password);
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
			_accountService.ClearReferenceData();
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
				if (!_accountService.CurrentAccount.DefaultCreditCard.HasValue)
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
					await _accountService.GetFacebookAccount(info.Id);
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
						await _accountService.GetTwitterAccount(info.Id);
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
