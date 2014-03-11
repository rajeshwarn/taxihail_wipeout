using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;
using ServiceStack.Text;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Mobile.Framework.Extensions;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.AppServices;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class CreateAccountViewModel: PageViewModel, ISubViewModel<RegisterAccount>
	{
		private readonly IAccountService _accountService;
		private readonly ITermsAndConditionsService _termsService;

		public CreateAccountViewModel(IAccountService accountService, ITermsAndConditionsService termsService)
		{
			_accountService = accountService;	
			_termsService = termsService;
		}

		public RegisterAccount Data { get; set; }
		public string ConfirmPassword { get; set; }

		public bool HasSocialInfo { get { return Data.FacebookId.HasValue () || Data.TwitterId.HasValue (); } }

		public void Init(string twitterId, string facebookId, string name, string email)
		{
			Data = new RegisterAccount
			{
				FacebookId = facebookId,
				TwitterId = twitterId,
				Name = name,
				Email = email,
			};
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

		private bool _termsAndConditionsAcknowledged;
		public bool TermsAndConditionsAcknowledged 
		{
			get { return !ShowTermsAndConditions || _termsAndConditionsAcknowledged; }
			set
			{
				_termsAndConditionsAcknowledged = value;
				RaisePropertyChanged();
			}
		}

		public ICommand NavigateToTermsAndConditions
		{
			get
			{
				return this.GetCommand(() => ShowViewModel<TermsAndConditionsViewModel>());
			}
		}

		public ICommand CreateAccount
		{
			get
			{
				return this.GetCommand(async () =>
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
						Data.AccountActivationDisabled = Settings.AccountActivationDisabled;
						try
						{
							await _accountService.Register(Data);
							if (!HasSocialInfo && !Data.AccountActivationDisabled)
							{
								this.Services().Message.ShowMessage(this.Services().Localize["AccountActivationTitle"], this.Services().Localize["AccountActivationMessage"]);
							}
							
							if(ShowTermsAndConditions)
							{
								// get the terms and conditions to make sure that when the user logs in
								// the acknowledgment of terms are up to date and if an error occurs, continue
								try
								{
									_termsService.GetTerms();
									_termsService.AcknowledgeTerms(true, Data.Email);
								}
								catch {}
							}

							this.ReturnResult(Data);

						}catch(Exception e)
						{
							var error = e.Message;
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
				});
			}			
		}
	}
}

