using System;
using System.Linq;
using Cirrious.MvvmCross.Interfaces.Commands;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using TinyIoC;

namespace apcurium.MK.Booking.Mobile.ViewModels.Callbox
{
    public class CallboxLoginViewModel : BaseViewModel
    {

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
                                          AccountService.ClearCache();
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
                    account = AccountService.GetAccount(Email, Password);
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
                    Password = string.Empty;
					if (AccountService.GetActiveOrdersStatus().Any(c => TinyIoCContainer.Current.Resolve<IBookingService>().IsCallboxStatusActive(c.IbsStatusId)))
                    {
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