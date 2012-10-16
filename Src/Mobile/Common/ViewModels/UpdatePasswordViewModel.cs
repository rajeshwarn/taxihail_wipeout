using Cirrious.MvvmCross.Commands;
using Cirrious.MvvmCross.Interfaces.Commands;
using apcurium.MK.Common.Entity;
using TinyIoC;
using TinyMessenger;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Extensions;
using System;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class UpdatePasswordViewModel : BaseViewModel
    {
		private IAccountService _accountService;
		private string _accountId;

		public UpdatePasswordViewModel( IAccountService accountService, string accountId )
        {
			_accountService = accountService;
			_accountId = accountId;
        }

        private string _currentPassword;
        public string CurrentPassword
        {
			get { return _currentPassword; }
            set
            {
				_currentPassword = value;
				FirePropertyChanged(() => CurrentPassword);
            }
        }

        private string _newPassword;
        public string NewPassword
        {
			get { return _newPassword; }
            set
            {
				_newPassword = value;
				FirePropertyChanged(() => NewPassword);
				FirePropertyChanged(() => NewPasswordIsConfirmed);
            }
        }

        private string _newPasswordConfirmation;
        public string NewPasswordConfirmation
        {
			get { return _newPasswordConfirmation; }
            set
            {
				_newPasswordConfirmation = value;
				FirePropertyChanged(() => NewPasswordConfirmation);
				FirePropertyChanged(() => NewPasswordIsConfirmed);
            }
        }

		public bool NewPasswordIsConfirmed { 
			get { return NewPassword == NewPasswordConfirmation; }
		}

		public bool CanUpdatePassword { 
			get { return !CurrentPassword.IsNullOrEmpty() && !NewPassword.IsNullOrEmpty() && NewPassword.Length >= 6 && NewPassword.Length <= 10 && NewPasswordIsConfirmed; }
		}

        public IMvxCommand UpdateCommand
        {
            get
            {
                
                return new MvxRelayCommand(() => {
					if (!CanUpdatePassword)
					{
						var title = TinyIoCContainer.Current.Resolve<IAppResource>().GetString("View_UpdatePassword");
						var msg = TinyIoCContainer.Current.Resolve<IAppResource>().GetString("CreateAccountInvalidPassword");
						TinyIoCContainer.Current.Resolve<IMessageService>().ShowMessage(title, msg);;
						return;
					}
					Guid accountId = new Guid();
					Guid.TryParse( _accountId, out accountId );
					var response = _accountService.UpdatePassword( accountId, CurrentPassword, NewPassword );
					if( response.ToUpper() == "OK" )
					{
						RequestClose(this);
					}

				});
            }
        }
    }
}