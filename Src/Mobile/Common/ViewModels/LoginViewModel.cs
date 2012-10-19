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
			_userInteractionEnabled = true;
		}

		private bool _userInteractionEnabled;
		public bool UserInteractionEnabled {
			get { return _userInteractionEnabled; }
			set { _userInteractionEnabled = value;
				FirePropertyChanged( () => UserInteractionEnabled );
			}
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
						TinyIoCContainer.Current.Resolve<IAccountService>().SignOut();
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
				TinyIoCContainer.Current.Resolve<IMessageService>().ShowProgress(true);
				UserInteractionEnabled = false;				
				var account = _accountService.GetAccount(Email, Password);

                //var dispatch = TinyIoC.TinyIoCContainer.Current.Resolve<IMvxViewDispatcherProvider>().Dispatcher;
                RequestNavigate<BookViewModel>(true );
                //dispatch.RequestNavigate(new MvxShowViewModelRequest(typeof(BookViewModel), null, false, MvxRequestedBy.UserAction));
				
			}
			finally
			{
				UserInteractionEnabled = true;
				TinyIoCContainer.Current.Resolve<IMessageService>().ShowProgress(false);

			}
		}

       

	}
}