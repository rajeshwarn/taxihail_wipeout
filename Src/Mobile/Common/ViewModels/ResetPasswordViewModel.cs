using System;
using System.Text.RegularExpressions;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Framework.Extensions;
using System.Net;

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
                return this.GetCommand(async () =>
                {
					if (!IsEmail(Email))
					{
						this.Services().Message.ShowMessage(this.Services().Localize["InvalidEmailTitle"],
															this.Services().Localize["InvalidEmailMessage"]);
						return;
					}

                    this.Services().Message.ShowProgress(true);

					try
					{
						await _accountService.ResetPassword(Email);
						this.ReturnResult(Email);
                    }
					catch(WebException e)
					{
						var title = this.Services().Localize["ServiceErrorCallTitle"];
						var msg = e.Message;
						if(String.Equals(e.Message, "NoNetwork"))
						{
								msg = this.Services().Localize["NoConnectionMessage"];
						}
						this.Services().Message.ShowMessage(title, msg);
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

