using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Commands;
using TinyIoC;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Api.Contract.Resources;
using System.Threading;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class LoginViewModel : BaseViewModel
    {
		private IAccountService _accountService;
		private IAppResource _appRessources;
		private IAppContext _appContext;

		public LoginViewModel(IAccountService accountService, IAppResource appResources, IAppContext appContext)
		{
			_accountService = accountService;
			_appRessources = appResources;
			_appContext = appContext;

            CheckVersion();

		}

        private void CheckVersion()
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                //The 2 second delay is required because the view might not be created.
                Thread.Sleep(2000);
                TinyIoCContainer.Current.Resolve<IApplicationInfoService>().CheckVersion();
            });

        }        
	

		private string _email;
		public string Email {
			get { return _email; }
			set { _email = value;
				FirePropertyChanged( () => Email );
			}
		}

		private string _password;
		public string Password {
			get { return _password; }
			set { _password = value;
				FirePropertyChanged( () => Password );
			}
		}

		public MvxRelayCommand SignInCommand
		{
			get
			{
				return new MvxRelayCommand(() => {
					try
					{
                        TinyIoCContainer.Current.Resolve<IAccountService>().ClearCache();
						ThreadPool.QueueUserWorkItem( SignIn );  
					}
					finally
					{
					}
				});
			}
		}

		private void SignIn( object state )
		{
			try
			{
                TinyIoCContainer.Current.Resolve<ILogger>().LogMessage("SignIn with server {0}", TinyIoCContainer.Current.Resolve<IAppSettings>().ServiceUrl);            
                var s =TinyIoCContainer.Current.Resolve<IMessageService>();
                s.ShowProgress(true);				
				var account = _accountService.GetAccount(Email, Password);

                if ( account != null )
                {
                    this.Password = "";
                    RequestNavigate<BookViewModel>(true );
                }
               
				
			}
			finally
			{
				
				TinyIoCContainer.Current.Resolve<IMessageService>().ShowProgress(false);

			}
		}

       

	}
}