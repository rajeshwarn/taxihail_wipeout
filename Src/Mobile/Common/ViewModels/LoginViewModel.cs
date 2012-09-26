using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Commands;
using TinyIoC;
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
						_appContext.SignOut();

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
				TinyIoCContainer.Current.Resolve<IMessageService>().ShowProgress(true);
				UserInteractionEnabled = false;
				string error = "";                      
				var account = _accountService.GetAccount(Email, Password, out error);
				if (account != null)
				{
					SetAccountInfo(account);
				}
				else
				{
					if (error.IsNullOrEmpty())
					{
						TinyIoCContainer.Current.Resolve<IMessageService>().ShowMessage( _appRessources.GetString("InvalidLoginMessageTitle"), _appRessources.GetString("AccountNotValidatedMessage"), _appRessources.GetString("ResendValidationButton"), () => _accountService.ResendConfirmationEmail(Email) );
					}
					else
					{
						TinyIoCContainer.Current.Resolve<IMessageService>().ShowMessage( _appRessources.GetString("InvalidLoginMessageTitle"), _appRessources.GetString("InvalidLoginMessage") + " (" + error + ")");
					}
				}
			}
			finally
			{
				UserInteractionEnabled = true;
				TinyIoCContainer.Current.Resolve<IMessageService>().ShowProgress(false);

			}
		}

		public void SetAccountInfo(Account account)
		{
			_appContext.LastEmail = Email;
			_appContext.LoggedInEmail = Email;
			InvokeOnMainThread(() => _appContext.UpdateLoggedInUser(account, false));
			RequestNavigate<BookViewModel>(true);

//			if( CanClose() )
//			{
//				this.Close();
//				//RequestClose( this );
//			}


//			if (_appContext.Controller.SelectedRefreshableViewController != null)
//			{
//				InvokeOnMainThread(() => { _appContext.Controller.SelectedRefreshableViewController.RefreshData(); });
//			}
		}

	}
}