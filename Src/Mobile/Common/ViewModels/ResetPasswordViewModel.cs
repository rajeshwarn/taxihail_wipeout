using System;
using System.Text.RegularExpressions;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Framework.Extensions;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class ResetPasswordViewModel : PageViewModel, ISubViewModel<string>
	{
		private readonly IAccountService _accountService;

		public ResetPasswordViewModel(IAccountService accountService)
		{
			_accountService = accountService;
		}

		public string Email { get; set; }

		public ICommand ResetPassword
		{
			get
			{
                return this.GetCommand(() =>
                {
					if (!IsEmail(Email))
					{
                        this.Services().Message.ShowMessage(this.Services().Localize["ResetPasswordInvalidDataTitle"], 
															this.Services().Localize["ResetPasswordInvalidDataMessage"]);
						return;
					}

                    this.Services().Message.ShowProgress(true);

					try
					{
						_accountService.ResetPassword(Email);
						this.ReturnResult(Email);
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

