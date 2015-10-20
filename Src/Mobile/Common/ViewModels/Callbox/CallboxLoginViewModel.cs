using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using TinyIoC;

namespace apcurium.MK.Booking.Mobile.ViewModels.Callbox
{
    public class CallboxLoginViewModel : BaseViewModel
    {
        private readonly IAccountService _accountService;

        public CallboxLoginViewModel(IAccountService accountService)
        {
            _accountService = accountService;
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
	                try
	                {
						_accountService.ClearCache();
	                }
	                catch (Exception ex)
	                {
		               Logger.LogError(ex);
	                }
                    
                    
					return SignIn();
                });
            }
        }

        private async Task SignIn()
        {
            try
            {
                Logger.LogMessage("SignIn with server {0}", Settings.ServiceUrl);
				this.Services().Message.ShowProgress(true);
                var account = default(Account);

                try
                {
                    account = await _accountService.SignIn(Email, Password);
                }
                catch (Exception e)
                {
                    var title = this.Services().Localize["InvalidLoginMessageTitle"];
                    var message = this.Services().Localize["InvalidLoginMessage"];

                    Logger.LogError( e );

					this.Services().Message.ShowMessage(title, message).FireAndForget();
                }

                if (account != null)
                {
                    Password = string.Empty;
                    if (_accountService.GetActiveOrdersStatus().Any(c => TinyIoCContainer.Current.Resolve<IBookingService>().IsCallboxStatusActive(c.IBSStatusId)))
                    {
						ShowViewModel<CallboxOrderListViewModel>();
                    }
                    else
                    {
						ShowViewModel<CallboxCallTaxiViewModel>();
                    }
                    Close(this);
                }
            }
            finally
            {
				this.Services().Message.ShowProgress(false);
            }
        }
    }
}