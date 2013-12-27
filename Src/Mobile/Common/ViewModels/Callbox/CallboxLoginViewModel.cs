using System;
using System.Linq;
using Cirrious.MvvmCross.Interfaces.Commands;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;

namespace apcurium.MK.Booking.Mobile.ViewModels.Callbox
{
    public class CallboxLoginViewModel : BaseViewModel
    {
        private IAccountService _accountService;

        public CallboxLoginViewModel(IAccountService accountService)
        {
            _accountService = accountService;
        }

        public override void Load()
        {
            base.Load();
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
                    var message = Resources.GetString("InvalidLoginMessage");

                    Logger.LogError( e );

                    MessageService.ShowMessage(title, message);
                }

                if (account != null)
                {
                    this.Password = string.Empty;
					if (_accountService.GetActiveOrdersStatus().Any(c => TinyIoC.TinyIoCContainer.Current.Resolve<IBookingService>().IsCallboxStatusActive(c.IbsStatusId)))
                    {
                        var active = _accountService.GetActiveOrdersStatus();
                        RequestNavigate<CallboxOrderListViewModel>(true);
                    }
                    else
                    {
                        RequestNavigate<CallboxCallTaxiViewModel>(true);
                    }
                    RequestClose(this);
                }
            }
            finally
            {
                MessageService.ShowProgress(false);
            }
        }
    }
}