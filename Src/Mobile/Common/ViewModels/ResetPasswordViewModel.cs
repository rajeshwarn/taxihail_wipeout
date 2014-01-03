using System;
using System.Text.RegularExpressions;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Framework.Extensions;
namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class ResetPasswordViewModel : BaseSubViewModel<string>
	{
        

        public ResetPasswordViewModel (string messageId) : base(messageId)
		{
		}

		public string Email { get; set; }

		public AsyncCommand ResetPassword
		{
			get
			{
                return GetCommand(() =>
                {
					if (!IsEmail(Email))
					{
                        this.Services().Message.ShowMessage(this.Services().Resources.GetString("ResetPasswordInvalidDataTitle"), 
                                                                    this.Services().Resources.GetString("ResetPasswordInvalidDataMessage"));
						return;
					}

                    this.Services().Message.ShowProgress(true);
					try{
                        this.Services().Account.ResetPassword(Email);
                        RequestClose( this );
                     }catch(Exception e)
                     {
                         var msg = this.Services().Resources.GetString("ServiceError" + e.Message);
                         var title = this.Services().Resources.GetString("ServiceErrorCallTitle");
                         this.Services().Message.ShowMessage(title, msg);
                     }finally
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

