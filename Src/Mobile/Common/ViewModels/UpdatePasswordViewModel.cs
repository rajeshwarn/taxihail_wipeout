using Cirrious.MvvmCross.Interfaces.Commands;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Common.Extensions;
using System;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using Cirrious.MvvmCross.ExtensionMethods;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class UpdatePasswordViewModel : BaseViewModel, IMvxServiceConsumer<IAccountService>
    {
		private readonly IAccountService _accountService;		

		public UpdatePasswordViewModel()
        {
            _accountService = this.GetService<IAccountService>();
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
			get { return !CurrentPassword.IsNullOrEmpty() 
                            && !NewPassword.IsNullOrEmpty() 
                            && NewPassword.Length >= 6 
                            && NewPassword.Length <= 10 
                            && NewPasswordIsConfirmed; }
		}

        public IMvxCommand UpdateCommand
        {
            get
            {

                return GetCommand(() =>
                {
					if (!CanUpdatePassword)
					{
                        var title = Resources.GetString("View_UpdatePassword");
                        var msg = Resources.GetString("CreateAccountInvalidPassword");
                        MessageService.ShowMessage(title, msg);
						return;
					}
                    MessageService.ShowProgress(true);
                    try{
                        _accountService.UpdatePassword( _accountService.CurrentAccount.Id, CurrentPassword, NewPassword );                        
                        _accountService.SignOut();							
                        var msg = Resources.GetString("ChangePasswordConfirmmation");
                        var title = Settings.ApplicationName;
						MessageService.ShowMessage(title, msg, () => RequestNavigate<LoginViewModel>(true));
                    }catch(Exception e)
                    {
                        var msg = Resources.GetString("ServiceError" + e.Message);
                        var title = Resources.GetString("ServiceErrorCallTitle");
                        MessageService.ShowMessage(title, msg);
                    }finally{
                        MessageService.ShowProgress(false);
                    }					

				});
            }
        }
    }
}