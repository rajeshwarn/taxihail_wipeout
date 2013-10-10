using System;
using Cirrious.MvvmCross.Interfaces.Commands;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Api.Contract.Requests;
using Cirrious.MvvmCross.Commands;
using apcurium.Framework.Extensions;
using System.Text.RegularExpressions;
using System.Linq;
using TinyIoC;
using apcurium.MK.Booking.Mobile.AppServices;
using TinyMessenger;
using ServiceStack.Text;

namespace apcurium.MK.Booking.Mobile
{
    public class CreateAcccountViewModel: BaseSubViewModel<RegisterAccount>
	{
		public RegisterAccount Data { get; set; }
		public string ConfirmPassword { get; set; }

		public bool HasSocialInfo { get { return Data.FacebookId.HasValue () || Data.TwitterId.HasValue (); } }

        public CreateAcccountViewModel (string messageId, string data) : base(messageId)
		{
			if (data != null) {
				Data = JsonSerializer.DeserializeFromString<RegisterAccount>(data);
			} else {
				Data = new RegisterAccount();
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

		public IMvxCommand Cancel
		{
			get
			{
                return GetCommand(() => { Close(); });
			}
		}

		public IMvxCommand CreateAccount
		{
			get
			{
                return GetCommand(() =>
                {
					
					if (!IsEmail(Data.Email))
					{
						MessageService.ShowMessage(Resources.GetString("ResetPasswordInvalidDataTitle"), Resources.GetString("ResetPasswordInvalidDataMessage"));
						return;
					}
					
					bool hasPassword = Data.Password.HasValue() && ConfirmPassword.HasValue();
                    if (Data.Email.IsNullOrEmpty() || Data.Name.IsNullOrEmpty() || Data.Phone.IsNullOrEmpty() || (!hasPassword && !HasSocialInfo))
					{
						MessageService.ShowMessage(Resources.GetString("CreateAccountInvalidDataTitle"), Resources.GetString("CreateAccountEmptyField"));
						return;
					}
					
					if (!HasSocialInfo && ((Data.Password != ConfirmPassword) || (Data.Password.Length < 6 || Data.Password.Length > 10)))
					{
						MessageService.ShowMessage(Resources.GetString("CreateAccountInvalidDataTitle"), Resources.GetString("CreateAccountInvalidPassword"));
						return;
					}
					
					if ( Data.Phone.Count(x => Char.IsDigit(x)) < 10 )
					{
						MessageService.ShowMessage(Resources.GetString("CreateAccountInvalidDataTitle"), Resources.GetString("InvalidPhoneErrorMessage"));
						return;
					}
					else
					{
						Data.Phone= new string(Data.Phone.ToArray().Where( c=> Char.IsDigit( c ) ).ToArray());
					}
												
					MessageService.ShowProgress(true);

					try
					{
						string error;
						
                        var setting = ConfigurationManager.GetSetting("AccountActivationDisabled");
                        Data.AccountActivationDisabled = bool.Parse(string.IsNullOrWhiteSpace(setting) ? bool.FalseString : setting);



						var result = TinyIoCContainer.Current.Resolve<IAccountService>().Register(Data, out error);
						if (result)
						{
							if (!HasSocialInfo && !Data.AccountActivationDisabled)
							{
								MessageService.ShowMessage(Resources.GetString("AccountActivationTitle"), Resources.GetString("AccountActivationMessage"));
							}
                            ReturnResult(Data);
						}
						else
						{
							if (error.Trim().IsNullOrEmpty())
							{
								error = Resources.GetString("CreateAccountErrorNotSpecified");
							}
							if (Resources.GetString("ServiceError" + error) != "ServiceError" + error)
							{
								MessageService.ShowMessage(Resources.GetString("CreateAccountErrorTitle"), Resources.GetString("CreateAccountErrorMessage") + " " + Resources.GetString("ServiceError" + error));
							}
							else
							{
								MessageService.ShowMessage(Resources.GetString("CreateAccountErrorTitle"), Resources.GetString("CreateAccountErrorMessage") + " " + error);
							}
						}
					}
					finally
					{
						MessageService.ShowProgress(false);
					}
				}
			);

			}			
		}
	}
}

