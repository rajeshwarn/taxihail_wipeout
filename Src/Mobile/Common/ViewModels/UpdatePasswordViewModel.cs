using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Common.Extensions;
using System;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.AppServices;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class UpdatePasswordViewModel : BaseViewModel
	{
		readonly IAccountService _accountService;

		public UpdatePasswordViewModel(IAccountService accountService)
		{
			_accountService = accountService;			
		}

		private string _currentPassword;
		public string CurrentPassword
		{
			get { return _currentPassword; }
			set
			{
				_currentPassword = value;
				RaisePropertyChanged();
			}
		}

		private string _newPassword;
		public string NewPassword
		{
			get { return _newPassword; }
			set
			{
				_newPassword = value;
				RaisePropertyChanged();
				RaisePropertyChanged(() => NewPasswordIsConfirmed);
			}
		}

		private string _Confirmation;
		public string Confirmation
		{
			get { return _Confirmation; }
			set
			{
				_Confirmation = value;
				RaisePropertyChanged();
				RaisePropertyChanged(() => NewPasswordIsConfirmed);
			}
		}

		public bool NewPasswordIsConfirmed { 
			get { return NewPassword == Confirmation; }
		}

		public bool CanUpdatePassword { 
			get { return !CurrentPassword.IsNullOrEmpty() 
				&& !NewPassword.IsNullOrEmpty() 
					&& NewPassword.Length >= 6 
						&& NewPassword.Length <= 10 
						&& NewPasswordIsConfirmed; }
		}

		public ICommand UpdateCommand
		{
			get
			{
				return this.GetCommand(() =>
				{
					if (!CanUpdatePassword)
					{
						var title = this.Services().Localize["View_UpdatePassword"];
						var msg = this.Services().Localize["CreateAccountInvalidPassword"];
						this.Services().Message.ShowMessage(title, msg);
						return;
					}
					this.Services().Message.ShowProgress(true);
					try
					{
						_accountService.UpdatePassword(_accountService.CurrentAccount.Id, CurrentPassword, NewPassword);
						_accountService.SignOut();
						var msg = this.Services().Localize["ChangePasswordConfirmmation"];
						var title = Settings.ApplicationName;
						this.Services().Message.ShowMessage(title, msg, () => ShowViewModel<LoginViewModel>());
					}
					catch(Exception e)
					{
						var msg = this.Services().Localize["ServiceError" + e.Message];
						var title = this.Services().Localize["ServiceErrorCallTitle"];
						this.Services().Message.ShowMessage(title, msg);
					}
					finally
					{
						this.Services().Message.ShowProgress(false);
					}					
				});
			}
		}

	}
}