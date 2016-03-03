using System;
using System.Text.RegularExpressions;
using System.Windows.Input;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Framework.Extensions;
using apcurium.MK.Common.Helpers;
using apcurium.MK.Common;
using System.Globalization;
using System.Net;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class CreateAccountViewModel : PageViewModel, ISubViewModel<RegisterAccount>
	{
		private readonly IRegisterWorkflowService _registerService;
		private readonly ITermsAndConditionsService _termsService;

		public CreateAccountViewModel(IRegisterWorkflowService registerService, ITermsAndConditionsService termsService)
		{
			_registerService = registerService;	
			_termsService = termsService;
		}

		public RegisterAccount Data { get; set; }
		
        public string ConfirmPassword { get; set; }

        public PhoneNumberModel PhoneNumber { get; set; }


		public bool HasSocialInfo { get { return Data.FacebookId.HasValue () || Data.TwitterId.HasValue (); } }

		public void Init(string twitterId, string facebookId, string name, string email)
		{
            string countryISOCode = new RegionInfo(CultureProvider.CultureInfo.LCID).TwoLetterISORegionName;

            PhoneNumber = new PhoneNumberModel()
            {
                Country = new CountryISOCode(countryISOCode)
            };

			Data = new RegisterAccount
			{
				FacebookId = facebookId,
				TwitterId = twitterId,
				Name = name,
				Email = email,
                Country = PhoneNumber.Country
			};
#if DEBUG
         
            // If we are using facebook id or twitterid we should not override the values provided by facebook or twitter.
           
            if (!facebookId.HasValue() && !twitterId.HasValue())
            {
                Data.Email = "testaccount@net.net";
                Data.Name = "test account";
                Data.Country = new CountryISOCode("CA");
                Data.Password = "password";
                ConfirmPassword = "password";
                return;
             }
            
             Data.Phone = "5147777777";
             PhoneNumber.Country = Data.Country;
             PhoneNumber.PhoneNumber = Data.Phone;
#endif
        }

        private bool _termsAndConditionsAcknowledged;

		public bool TermsAndConditionsAcknowledged 
		{
			get { return !Settings.ShowTermsAndConditions || _termsAndConditionsAcknowledged; }
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
                    Data.Phone = PhoneNumber.PhoneNumber;
                    Data.Country = PhoneNumber.Country;

					if (!EmailHelper.IsEmail(Data.Email))
					{
						await this.Services().Message.ShowMessage(this.Services().Localize["InvalidEmailTitle"], this.Services().Localize["InvalidEmailMessage"]);
						return;
					}
					
					var hasPassword = Data.Password.HasValue() && ConfirmPassword.HasValue();
                    if (Data.Email.IsNullOrEmpty() || Data.Name.IsNullOrEmpty() || Data.Phone.IsNullOrEmpty() || (!hasPassword && !HasSocialInfo))
					{
                        await this.Services().Message.ShowMessage(this.Services().Localize["CreateAccountInvalidDataTitle"], this.Services().Localize["CreateAccountEmptyField"]);
						return;
					}
					
					if (!HasSocialInfo && ((Data.Password != ConfirmPassword) || (Data.Password.Length < 6)))
					{
                        await this.Services().Message.ShowMessage(this.Services().Localize["CreateAccountInvalidDataTitle"], this.Services().Localize["CreateAccountInvalidPassword"]);
						return;
					}

                    if (!PhoneNumber.IsNumberPossible())
					{
                        await this.Services().Message.ShowMessage(this.Services().Localize["CreateAccountInvalidDataTitle"],
                            string.Format(this.Services().Localize["InvalidPhoneErrorMessage"], PhoneNumber.GetPhoneExample()));
						return;
					}

				    if (Settings.IsPayBackRegistrationFieldRequired == true && !Data.PayBack.HasValue())
				    {
                        await this.Services().Message.ShowMessage(this.Services().Localize["CreateAccountInvalidDataTitle"], this.Services().Localize["NoPayBackErrorMessage"]);
                        return;
				    }

                    if (Data.PayBack.HasValue() && (Data.PayBack.Length > 10 || !Data.PayBack.IsNumber()))
				    {
                        await this.Services().Message.ShowMessage(this.Services().Localize["CreateAccountInvalidDataTitle"], this.Services().Localize["InvalidPayBackErrorMessage"]);
                        return;
				    }

                    Data.Phone = PhoneHelper.GetDigitsFromPhoneNumber(Data.Phone);

                    this.Services().Message.ShowProgress(true);

					try
					{	
						try
						{
                            // PayBack value is set to string empty if the field is left empty by the user
						    Data.PayBack = Data.PayBack == string.Empty ? null : Data.PayBack;

							await _registerService.RegisterAccount(Data);

                            if (!HasSocialInfo && !Settings.AccountActivationDisabled)
							{
								if(Settings.SMSConfirmationEnabled)
								{
									ShowViewModelAndRemoveFromHistory<AccountConfirmationViewModel>(null);								
								}
                                else
								{
									this.Services().Message.ShowMessage(
                                        this.Services().Localize["AccountActivationTitle"], 
										this.Services().Localize["AccountActivationMessage"],
                                        () => Close(this));
								}
							}
                            else
							{
								Close(this);
								_registerService.RegistrationFinished();
							}
							
							if(Settings.ShowTermsAndConditions)
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
						}
                        catch(Exception e)
						{
							var error = e.Message;
							if (error.Trim().IsNullOrEmpty())
							{
								error = this.Services().Localize["CreateAccountErrorNotSpecified"];
							}
							else if (String.Equals(error, "NoNetwork"))
							{
								error = this.Services().Localize["NoConnectionMessage"];
							}
							if (this.Services().Localize[error] != error)
							{
								this.Services().Message.ShowMessage(this.Services().Localize["CreateAccountErrorTitle"], this.Services().Localize["CreateAccountErrorMessage"] + " " + this.Services().Localize[error]);
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

