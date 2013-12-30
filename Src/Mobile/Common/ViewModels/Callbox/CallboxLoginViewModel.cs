using System;
using System.Linq;
using Cirrious.MvvmCross.Interfaces.Commands;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Extensions;

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
										 this.Services().Account.ClearCache();
                                          SignIn();
                                      });
            }
        }

        private void SignIn()
        {
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
					var title = this.Services().Resources.GetString("InvalidLoginMessageTitle");
					var message = this.Services().Resources.GetString("InvalidLoginMessage");

                    Logger.LogError( e );

					this.Services().Message.ShowMessage(title, message);
                }

                if (account != null)
                {
                    Password = string.Empty;
                    if (this.Services().Account.GetActiveOrdersStatus().Any(c => TinyIoCContainer.Current.Resolve<IBookingService>().IsCallboxStatusActive(c.IbsStatusId)))
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
				this.Services().Message.ShowProgress(false);
            }
        }
    }
}