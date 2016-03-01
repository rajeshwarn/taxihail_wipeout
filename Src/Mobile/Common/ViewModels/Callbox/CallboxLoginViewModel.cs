using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Common.Configuration;

namespace apcurium.MK.Booking.Mobile.ViewModels.Callbox
{
    public class CallboxLoginViewModel : BaseViewModel
    {
        private readonly IAccountService _accountService;
	    private readonly IBookingService _bookingService;

        public CallboxLoginViewModel(IAccountService accountService, IBookingService bookingService)
        {
	        _accountService = accountService;
	        _bookingService = bookingService;
        }

	    public override void Start()
        {
#if DEBUG
            Email = "john@taxihail.com";
            Password = "password";
#endif
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

        public ICommand SignInCommand
        {
            get
            {
                return this.GetCommand(() =>
                {
					_accountService.ClearCache();

					return SignIn();
                });
            }
        }

        public async void SetServerUrl(string serverUrl)
        {
            using (this.Services().Message.ShowProgress())
            {
                await InnerSetServerUrl(serverUrl);
            }
        }

        private async Task InnerSetServerUrl(string serverUrl)
        {
            await Container.Resolve<IAppSettings>().ChangeServerUrl(serverUrl);
            this.Services().ApplicationInfo.ClearAppInfo();
            _accountService.ClearReferenceData();
        }

        private async Task SignIn()
        {
            try
            {
                Logger.LogMessage("SignIn with server {0}", Container.Resolve<IAppSettings>().GetServiceUrl());
				this.Services().Message.ShowProgress(true);

                var account = await _accountService.SignIn(Email, Password);

                if (account != null)
                {
                    Password = string.Empty;

                    var activeOrders = await _accountService.GetActiveOrdersStatus();

                    if (activeOrders.Any(c => _bookingService.IsCallboxStatusActive(c.IBSStatusId)))
                    {
						ShowViewModelAndRemoveFromHistory<CallboxOrderListViewModel>();
                    }
                    else
                    {
                        ShowViewModelAndRemoveFromHistory<CallboxCallTaxiViewModel>();
                    }
                }
            }
            catch (AuthException e)
            {
                var localize = this.Services().Localize;
                switch (e.Failure)
                {
                    case AuthFailure.InvalidServiceUrl:
                    case AuthFailure.InvalidUsernameOrPassword:
                    {
                        var title = localize["InvalidLoginMessageTitle"];
                        var message = localize["InvalidLoginMessage"];
                        this.Services().Message.ShowMessage(title, message);
                    }
                    break;
                    case AuthFailure.AccountDisabled:
                    {
                        var title = this.Services().Localize["InvalidLoginMessageTitle"];
                        var message = localize["AccountDisabled_NoCall"];
                        this.Services().Message.ShowMessage(title, message);
                    }
                    break;
                    case AuthFailure.AccountNotActivated:
                    {
                        var title = localize["InvalidLoginMessageTitle"];
                        var message = localize["AccountNotActivated"];

                        this.Services().Message.ShowMessage(title, message);
                    }
                    break;
                }
            }
            finally
            {
				this.Services().Message.ShowProgress(false);
            }
        }
    }
}