using System;
using Cirrious.MvvmCross.Interfaces.Commands;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Commands;
using apcurium.Framework.Extensions;
using System.Text.RegularExpressions;
using TinyIoC;
using apcurium.MK.Booking.Mobile.AppServices;

namespace apcurium.MK.Booking.Mobile
{
    public class ResetPasswordViewModel : BaseSubViewModel<string>
	{
        public ResetPasswordViewModel (string messageId) : base(messageId)
		{
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
					try
					{
						var result = TinyIoCContainer.Current.Resolve<IAccountService>().ResetPassword(Email);
						if (result)
						{
							MessageService.ShowMessage(Resources.GetString("ResetPasswordConfirmationTitle"), Resources.GetString("ResetPasswordConfirmationMessage"));
                            ReturnResult(Email);
						}
						else
						{
							MessageService.ShowMessage(Resources.GetString("NoConnectionTitle"), Resources.GetString("NoConnectionMessage"));
						}
					}
					finally
					{
						MessageService.ShowProgress(false);
					}
				});
			}
			
		}

		public IMvxCommand Cancel
		{
			get
			{
                return GetCommand(() => { Close(); });
		    }
	    }

		private bool IsEmail(string inputEmail)
		{
			inputEmail = inputEmail.ToSafeString();
			string strRegex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" + @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" + @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
			var re = new Regex(strRegex);
			if (re.IsMatch(inputEmail))
				return (true);
			else
				return (false);
		}
	}
}

