using System;
using System.Text.RegularExpressions;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Framework.Extensions;
using Cirrious.MvvmCross.ExtensionMethods;
using Cirrious.MvvmCross.Interfaces.Commands;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class ResetPasswordViewModel : BaseSubViewModel<string>, IMvxServiceConsumer<IAccountService>
	{
        readonly IAccountService _accountService;

        public ResetPasswordViewModel (string messageId) : base(messageId)
		{
            _accountService = this.GetService<IAccountService>();
		}

		public string Email { get; set; }

		public IMvxCommand ResetPassword
		{
			get
			{
                return GetCommand(() =>
                {
					if (!IsEmail(Email))
					{
						MessageService.ShowMessage(Resources.GetString("ResetPasswordInvalidDataTitle"), Resources.GetString("ResetPasswordInvalidDataMessage"));
						return;
					}				
					
					MessageService.ShowProgress(true);
					try{
                        _accountService.ResetPassword(Email);
                        RequestClose( this );
                     }catch(Exception e)
                     {
                        var msg = Resources.GetString("ServiceError" + e.Message);
                        var title = Resources.GetString("ServiceErrorCallTitle");
                        MessageService.ShowMessage(title, msg);
                     }finally
					 {
						MessageService.ShowProgress(false);
					 }
				});
			}
			
		}

		private bool IsEmail(string inputEmail)
		{
			inputEmail = inputEmail.ToSafeString();
			const string strRegex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" + @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" + @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
			var re = new Regex(strRegex);
			if (re.IsMatch(inputEmail))
				return (true);
		    return (false);
		}
	}
}

