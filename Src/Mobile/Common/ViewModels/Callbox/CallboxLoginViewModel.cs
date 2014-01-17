using System;
using System.Linq;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Extensions;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.ViewModels.Callbox
{
    public class CallboxLoginViewModel : BaseViewModel
    {

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

        private async Task SignIn()
        {
            try
            {
				Logger.LogMessage("SignIn with server {0}", this.Services().Settings.ServiceUrl);
				this.Services().Message.ShowProgress(true);
                var account = default(Account);

                try
                {
                    account = await this.Services().Account.SignIn(Email, Password);
                }
                catch (Exception e)
                {
                    var title = this.Services().Localize["InvalidLoginMessageTitle"];
                    var message = this.Services().Localize["InvalidLoginMessage"];

                    Logger.LogError( e );

					this.Services().Message.ShowMessage(title, message);
                }

                if (account != null)
                {
                    Password = string.Empty;
                    if (this.Services().Account.GetActiveOrdersStatus().Any(c => TinyIoCContainer.Current.Resolve<IBookingService>().IsCallboxStatusActive(c.IbsStatusId)))
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