using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Common.Extensions;
using System;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class UpdatePasswordViewModel : BaseViewModel
    {
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

        private string _newPasswordConfirmation;
        public string NewPasswordConfirmation
        {
			get { return _newPasswordConfirmation; }
            set
            {
				_newPasswordConfirmation = value;
				RaisePropertyChanged();
				RaisePropertyChanged(() => NewPasswordIsConfirmed);
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

        public AsyncCommand UpdateCommand
        {
            get
            {

                return GetCommand(() =>
                {
					if (!CanUpdatePassword)
					{
                        var title = this.Services().Resources.GetString("View_UpdatePassword");
                        var msg = this.Services().Resources.GetString("CreateAccountInvalidPassword");
                        this.Services().Message.ShowMessage(title, msg);
						return;
					}
                    this.Services().Message.ShowProgress(true);
                    try{
                        this.Services().Account.UpdatePassword(this.Services().Account.CurrentAccount.Id, CurrentPassword, NewPassword);
                        this.Services().Account.SignOut();
                        var msg = this.Services().Resources.GetString("ChangePasswordConfirmmation");
                        var title = this.Services().Settings.ApplicationName;
                        this.Services().Message.ShowMessage(title, msg, () => ShowViewModel<LoginViewModel>(true));
                    }catch(Exception e)
                    {
                        var msg = this.Services().Resources.GetString("ServiceError" + e.Message);
                        var title = this.Services().Resources.GetString("ServiceErrorCallTitle");
                        this.Services().Message.ShowMessage(title, msg);
                    }finally{
                        this.Services().Message.ShowProgress(false);
                    }					

				});
            }
        }
    }
}