using System;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Api.Contract.Requests;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Common.Extensions;
using ServiceStack.ServiceClient.Web;
using apcurium.MK.Common;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Diagnostic;
using System.Globalization;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class AccountConfirmationViewModel : PageViewModel
	{
		private readonly IRegisterWorkflowService _registerService;
		private IAccountServiceClient _accountServiceClient;
		private RegisterAccount _account;
		private ILogger _logger;

		public AccountConfirmationViewModel(IRegisterWorkflowService registerService, IAccountServiceClient accountServiceClient, ILogger logger)
		{
			_registerService = registerService;
			_accountServiceClient = accountServiceClient;
			_logger = logger;
			PhoneNumber = new PhoneNumberModel();
		}


		public async void Init()
		{
			_account = _registerService.Account;

			CurrentAccountPhoneResponse currentAccountPhone = null;

			try
			{
				currentAccountPhone = await _accountServiceClient.GetAccountPhoneNumber(new CurrentAccountPhoneRequest() { Email = _account.Email });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex);

				string countryISOCode = new RegionInfo(CultureProvider.CultureInfo.LCID).TwoLetterISORegionName;

				CountryCode.GetCountryCodeByIndex(CountryCode.GetCountryCodeIndexByCountryISOCode(countryISOCode));

				currentAccountPhone = new CurrentAccountPhoneResponse()
				{
					CountryCode = CountryCode.GetCountryCodeByIndex(CountryCode.GetCountryCodeIndexByCountryISOCode(countryISOCode)).CountryISOCode,
					PhoneNumber = ""
				};
			}

			_account.Country = currentAccountPhone.CountryCode;
			_account.Phone = currentAccountPhone.PhoneNumber;

			PhoneNumber.Country = currentAccountPhone.CountryCode;
			PhoneNumber.PhoneNumber = currentAccountPhone.PhoneNumber;

			RaisePropertyChanged(() => Phone);
			RaisePropertyChanged(() => SelectedCountryCode);
			RaisePropertyChanged(() => PhoneNumber);
		}

		public string Code { get; set; }

		public PhoneNumberModel PhoneNumber { get; set; }

		public CountryCode[] CountryCodes
		{
			get
			{
				return CountryCode.CountryCodes;
			}
		}

		public CountryCode SelectedCountryCode
		{
			get
			{
				return CountryCode.GetCountryCodeByIndex(CountryCode.GetCountryCodeIndexByCountryISOCode(_account.Country));
			}

			set
			{
				_account.Country = value.CountryISOCode;
				PhoneNumber.Country = value.CountryISOCode;
				RaisePropertyChanged();
			}
		}

		public string Phone
		{
			get
			{
				return _account.Phone;
			}
			set
			{
				_account.Phone = value;
				PhoneNumber.PhoneNumber = value;
				RaisePropertyChanged();
			}
		}




		public ICommand ConfirmAccount 
		{
			get 
			{
				return this.GetCommand (async () => 
				{
					if(Code.HasValue())
					{
						try{
							await _registerService.ConfirmAccount(Code);
							_account.IsConfirmed = true;
							Close(this);
							_registerService.RegistrationFinished();

						}catch(WebServiceException e)
						{
							var errorMessage = this.Services().Localize["ServiceError" + e.Message];
							if(errorMessage == "ServiceError" + e.Message)
							{
								errorMessage = e.Message;
							}

							this.Services().Message.ShowMessage(this.Services().Localize["AccountConfirmation_ErrorTitle"], errorMessage);
						}
					}
				});
			}
		}

        public ICommand ResendConfirmationCode
        {
            get
            {
                return this.GetCommand(async () =>
                {
					try
					{
						await _registerService.GetConfirmationCode(SelectedCountryCode.CountryISOCode, Phone);
						this.Services().Message.ShowMessage(this.Services().Localize["ResendConfirmationCodeTitle"],
							this.Services().Localize["ResendConfirmationCodeText"]);
					}
					catch (ArgumentException exception)
					{
						_logger.LogError(exception);
					}
                });
            }
        }
	}
}