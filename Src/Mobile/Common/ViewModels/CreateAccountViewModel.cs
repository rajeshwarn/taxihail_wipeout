using System;
using System.Linq;
using System.Text.RegularExpressions;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.AppServices.Social;
using apcurium.MK.Booking.Mobile.Framework.Extensions;
using ServiceStack.Text;
using TinyIoC;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class CreateAccountViewModel: BaseSubViewModel<RegisterAccount>
	{
		readonly IFacebookService _facebookService;

		public RegisterAccount Data { get; set; }
		public string ConfirmPassword { get; set; }

		public bool HasSocialInfo { get { return Data.FacebookId.HasValue () || Data.TwitterId.HasValue (); } }

		public CreateAccountViewModel()
		{
			_facebookService = TinyIoCContainer.Current.Resolve<IFacebookService>();
		}

        public new void Init(string data)
		{
			if (data != null)
			{
				Data = JsonSerializer.DeserializeFromString<RegisterAccount>(data);
			} else
			{
				Data = new RegisterAccount();
			}
		}

		private bool IsEmail(string inputEmail)
		{
			inputEmail = inputEmail.ToSafeString();
			const string strRegex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" + @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" + @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
			var re = new Regex(strRegex);
		    if (re.IsMatch(inputEmail))
		    {
		        return (true);
		    }
		    return (false);
		}

        public AsyncCommand CreateAccount
		{
			get
			{
                return GetCommand(() =>
                {
					if (!IsEmail(Data.Email))
					{
                        this.Services().Message.ShowMessage(this.Services().Localize["ResetPasswordInvalidDataTitle"], this.Services().Localize["ResetPasswordInvalidDataMessage"]);
						return;
					}
					
					bool hasPassword = Data.Password.HasValue() && ConfirmPassword.HasValue();
                    if (Data.Email.IsNullOrEmpty() || Data.Name.IsNullOrEmpty() || Data.Phone.IsNullOrEmpty() || (!hasPassword && !HasSocialInfo))
					{
                        this.Services().Message.ShowMessage(this.Services().Localize["CreateAccountInvalidDataTitle"], this.Services().Localize["CreateAccountEmptyField"]);
						return;
					}
					
					if (!HasSocialInfo && ((Data.Password != ConfirmPassword) || (Data.Password.Length < 6 || Data.Password.Length > 10)))
					{
                        this.Services().Message.ShowMessage(this.Services().Localize["CreateAccountInvalidDataTitle"], this.Services().Localize["CreateAccountInvalidPassword"]);
						return;
					}
					
					if ( Data.Phone.Count(x => Char.IsDigit(x)) < 10 )
					{
                        this.Services().Message.ShowMessage(this.Services().Localize["CreateAccountInvalidDataTitle"], this.Services().Localize["InvalidPhoneErrorMessage"]);
						return;
					}
                    Data.Phone= new string(Data.Phone.ToArray().Where( c=> Char.IsDigit( c ) ).ToArray());

                    this.Services().Message.ShowProgress(true);

					try
					{
                        var showTermsAndConditions = this.Services().Config.GetSetting("Client.ShowTermsAndConditions", false);
                        if( showTermsAndConditions && !_termsAndConditionsApproved )
                        {
							ShowSubViewModel<TermsAndConditionsViewModel, bool>( null, OnTermsAndConditionsCallback);
                            return;
                        }

						string error;

                        var setting = this.Services().Config.GetSetting("AccountActivationDisabled");
                        Data.AccountActivationDisabled = bool.Parse(string.IsNullOrWhiteSpace(setting) ? bool.FalseString : setting);

                        var result = this.Services().Account.Register(Data, out error);
						if (result)
						{
							if (!HasSocialInfo && !Data.AccountActivationDisabled)
							{
                                this.Services().Message.ShowMessage(this.Services().Localize["AccountActivationTitle"], this.Services().Localize["AccountActivationMessage"]);
							}
                            ReturnResult(Data);
						}
						else
						{
							if (error.Trim().IsNullOrEmpty())
							{
                                error = this.Services().Localize["CreateAccountErrorNotSpecified"];
							}
                            if (this.Services().Localize["ServiceError" + error] != "ServiceError" + error)
							{
                                this.Services().Message.ShowMessage(this.Services().Localize["CreateAccountErrorTitle"], this.Services().Localize["CreateAccountErrorMessage"] + " " + this.Services().Localize["ServiceError" + error]);
							}
							else
							{
                                this.Services().Message.ShowMessage(this.Services().Localize["CreateAccountErrorTitle"], this.Services().Localize["CreateAccountErrorMessage"] + " " + error);
							}
						}
					}
					finally
					{
                        this.Services().Message.ShowProgress(false);
					}
				}
			);

			}			
		}

        private bool _termsAndConditionsApproved;
        private void OnTermsAndConditionsCallback(bool approved)
        {
            _termsAndConditionsApproved = approved;
            if (approved && CreateAccount.CanExecute())
            {
                CreateAccount.Execute();
            }            
        }
	}
}

