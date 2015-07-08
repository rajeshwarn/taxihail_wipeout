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

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class PhoneNumberChangedEventArgs : EventArgs
    {
        public CountryISOCode Country { get; set; }

        public string PhoneNumber { get; set; }
    }

    public class PhoneNumberInfo
    {
        CountryISOCode country;

        string phoneNumber;

        public CountryISOCode Country
        {
            get
            {
                return country;
            }

            set
            {
                country = value;
                PhoneNumberDatasourceChangedCallEvent();
            }
        }

        public string PhoneNumber
        {
            get
            {
                return phoneNumber;
            }
            set
            {
                phoneNumber = value;
                PhoneNumberDatasourceChangedCallEvent();
            }
        }

        public delegate void PhoneNumberDatasourceChangedEventHandler(object sender, PhoneNumberChangedEventArgs e);

        public event PhoneNumberDatasourceChangedEventHandler PhoneNumberDatasourceChanged;

        public void PhoneNumberDatasourceChangedCallEvent()
        {
            if (PhoneNumberDatasourceChanged != null)
                PhoneNumberDatasourceChanged(this, new PhoneNumberChangedEventArgs() { Country = country, PhoneNumber = phoneNumber });
        }

        public void NotifyChanges(object sender, PhoneNumberChangedEventArgs e)
        {
            this.country = e.Country;
            
			if (e.PhoneNumber != null)
			{
				this.phoneNumber = e.PhoneNumber;
			}
        }
    }
    
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

        public PhoneNumberInfo PhoneNumber { get; set; }


		public bool HasSocialInfo { get { return Data.FacebookId.HasValue () || Data.TwitterId.HasValue (); } }

		public void Init(string twitterId, string facebookId, string name, string email)
		{
            string countryISOCode = new RegionInfo(CultureProvider.CultureInfo.LCID).TwoLetterISORegionName;

            PhoneNumber = new PhoneNumberInfo()
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
			Data.Email = "testaccount@net.net";
			Data.Name = "test account" ;
            Data.Country = new CountryISOCode("CA");
			Data.Phone = "5147777777";
			Data.Password = "password";
			ConfirmPassword = "password";
            PhoneNumber.Country = Data.Country;
            PhoneNumber.PhoneNumber = Data.Phone;
			#endif
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

					if (!IsEmail(Data.Email))
					{
                        await this.Services().Message.ShowMessage(this.Services().Localize["ResetPasswordInvalidDataTitle"], this.Services().Localize["ResetPasswordInvalidDataMessage"]);
						return;
					}
					
					var hasPassword = Data.Password.HasValue() && ConfirmPassword.HasValue();
                    if (Data.Email.IsNullOrEmpty() || Data.Name.IsNullOrEmpty() || Data.Phone.IsNullOrEmpty() || (!hasPassword && !HasSocialInfo))
					{
                        await this.Services().Message.ShowMessage(this.Services().Localize["CreateAccountInvalidDataTitle"], this.Services().Localize["CreateAccountEmptyField"]);
						return;
					}
					
					if (!HasSocialInfo && ((Data.Password != ConfirmPassword) || (Data.Password.Length < 6 || Data.Password.Length > 10)))
					{
                        await this.Services().Message.ShowMessage(this.Services().Localize["CreateAccountInvalidDataTitle"], this.Services().Localize["CreateAccountInvalidPassword"]);
						return;
					}

                    if (!libphonenumber.PhoneNumberUtil.Instance.IsPossibleNumber(Data.Phone, Data.Country.Code))
					{
                        libphonenumber.PhoneNumber phoneNumberExample = libphonenumber.PhoneNumberUtil.Instance.GetExampleNumber(Data.Country.Code);
                        string phoneNumberExampleText = phoneNumberExample.FormatInOriginalFormat(Data.Country.Code);

                        await this.Services().Message.ShowMessage(this.Services().Localize["CreateAccountInvalidDataTitle"], string.Format(this.Services().Localize["InvalidPhoneErrorMessage"], phoneNumberExampleText));
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

