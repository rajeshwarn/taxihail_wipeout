﻿using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Helpers;
using apcurium.MK.Common;
using MK.Common.Exceptions;
using System.Reactive.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources;
using System.Collections.Generic;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class RideSettingsViewModel: PageViewModel
    {
		private readonly IAccountService _accountService;
		private readonly IVehicleTypeService _vehicleTypeService;
		private readonly IPaymentService _paymentService;
	    private readonly IAccountPaymentService _accountPaymentService;
		private readonly IOrderWorkflowService _orderWorkflowService;
		private readonly INetworkRoamingService _networkRoamingService;

        private BookingSettings _bookingSettings;
	    private ClientPaymentSettings _paymentSettings;

		private string _email;

		public RideSettingsViewModel(IAccountService accountService, 
			IVehicleTypeService vehicleTypeService,
			IPaymentService paymentService,
            IAccountPaymentService accountPaymentService,
			IOrderWorkflowService orderWorkflowService,
			INetworkRoamingService networkRoamingService)
		{
			_vehicleTypeService = vehicleTypeService;
			_orderWorkflowService = orderWorkflowService;
			_paymentService = paymentService;
		    _accountPaymentService = accountPaymentService;
			_accountService = accountService;
			_networkRoamingService = networkRoamingService;

            _payments = new ListItem[0];

            PhoneNumber = new PhoneNumberModel();

			Observe(_accountService.GetAndObservePaymentsList(), paymentTypes => PaymentTypesChanged(paymentTypes).FireAndForget());
		}

		private async Task PaymentTypesChanged(IList<ListItem> paymentList)
		{
			Payments = paymentList
				.Select(x => new ListItem { Id = x.Id, Display = this.Services().Localize[x.Display] })
				.ToArray();

			await HandleChargeTypeSelectionAccess();

			RaisePropertyChanged(() => ChargeTypeId);
			RaisePropertyChanged(() => ChargeTypeName);
			RaisePropertyChanged(() => IsChargeAccountPaymentEnabled);
		}

		public async void Init()
        {
		    using (this.Services ().Message.ShowProgress ())
			{
			    try
			    {
					_bookingSettings = _accountService.CurrentAccount.Settings;
					_email = _accountService.CurrentAccount.Email;
                    _paymentSettings = await _paymentService.GetPaymentSettings();

					PhoneNumber.Country = _bookingSettings.Country;
                    PhoneNumber.PhoneNumber = _bookingSettings.Phone;

                    RaisePropertyChanged(() => ChargeTypeId);
                    RaisePropertyChanged(() => ChargeTypeName);
                    RaisePropertyChanged(() => IsChargeAccountPaymentEnabled);
                    RaisePropertyChanged(() => IsPayBackFieldEnabled);
					RaisePropertyChanged(() => Email);
					RaisePropertyChanged(() => PhoneNumber);
					RaisePropertyChanged(() => SelectedCountryCode);

                    // this should be called last since it calls the server, we don't want to slow down other controls
					var v = await _vehicleTypeService.GetAndObserveVehiclesList().Take(1).ToTask();
                    _vehicles = v == null ? new ListItem[0] : v.Select(x => new ListItem { Id = x.ReferenceDataVehicleId, Display = x.Name }).ToArray();
                    RaisePropertyChanged(() => Vehicles);
                    RaisePropertyChanged(() => VehicleTypeId);
                    RaisePropertyChanged(() => VehicleTypeName);
			    }
			    catch (Exception ex)
			    {
					Logger.LogMessage(ex.Message, ex.ToString());
					this.Services().Message.ShowMessage(this.Services().Localize["Error"], this.Services().Localize["RideSettingsLoadError"]);
			    }
			}
		}

		private async Task HandleChargeTypeSelectionAccess()
		{
			var marketSettings = await _networkRoamingService.GetAndObserveMarketSettings().Take(1).ToTask();
			var isLocalMarket = marketSettings.IsLocalMarket;

			// We ignore the DisableChargeTypeWhenCardOnFile when on external market because the override in marketSetting will decide if we can change the charge type.
			if (!isLocalMarket)
			{
				IsChargeTypesEnabled = Payments.Length > 1;
				return;
			}

			// If the setting DisableChargeTypeWhenCardOnFile is true, prevent changing the chargetype to something else then credit card.
			var isChargeTypeLocked = _accountService.CurrentAccount.DefaultCreditCard != null && Settings.DisableChargeTypeWhenCardOnFile;

			IsChargeTypesEnabled = !isChargeTypeLocked && Payments.Length > 1;
		}

	    public bool IsChargeTypesEnabled
	    {
	        get { return _isChargeTypesEnabled; }
	        set
	        {
	            _isChargeTypesEnabled = value;
	            RaisePropertyChanged();
	        }
	    }

	    public bool IsChargeAccountPaymentEnabled
	    {
	        get
	        {
                return _paymentSettings.IsChargeAccountPaymentEnabled;
	        }
	    }

        public bool IsPayBackFieldEnabled
        {
            get
            {
                return Settings.IsPayBackRegistrationFieldRequired.HasValue;
            }
        }

		public bool IsVehicleTypeSelectionEnabled
		{
			get
			{
				return Settings.VehicleTypeSelectionEnabled;
			}
		}

        private PaymentDetailsViewModel _paymentPreferences;
        public PaymentDetailsViewModel PaymentPreferences
        {
            get
            {
                if (_paymentPreferences == null)
                {
					_paymentPreferences = Container.Resolve<PaymentDetailsViewModel>();
					_paymentPreferences.Start();
                }
                return _paymentPreferences;
            }
        }

        private ListItem[] _vehicles;
        public ListItem[] Vehicles
        {
            get
            {
                return _vehicles;
            }
        }

		private ListItem[] _payments;
	    private bool _isChargeTypesEnabled;

	    public ListItem[] Payments
        {
            get
            {
				return _payments;
            }
            set
            {
                _payments = value ?? new ListItem[0];

                RaisePropertyChanged();
            }
        }

	    public int? VehicleTypeId
        {
            get
            {
                return _bookingSettings.VehicleTypeId;
            }
            set
            {
                if (value != _bookingSettings.VehicleTypeId)
                {
                    _bookingSettings.VehicleTypeId = value;
					RaisePropertyChanged();
					RaisePropertyChanged(() => VehicleTypeName);
					_orderWorkflowService.SetVehicleType (value);
                }
            }
        }

        public string VehicleTypeName
        {
            get
            {
                if (!VehicleTypeId.HasValue)
                {
                    return this.Services().Localize["NoPreference"];
                }

                if (Vehicles == null)
                {
                    return null;
                }

                var vehicle = Vehicles.FirstOrDefault(x => x.Id == VehicleTypeId);
				if (vehicle == null)
				{
					vehicle = Vehicles.FirstOrDefault();
				}
                return vehicle.Display;
            }
        }

        public int? ChargeTypeId
        {
            get
            {
                return _bookingSettings.ChargeTypeId;
            }
            set
            {
                if (value != _bookingSettings.ChargeTypeId)
                {
                    _bookingSettings.ChargeTypeId = value;
					RaisePropertyChanged();
					RaisePropertyChanged(() => ChargeTypeName);
                }
            }
        }

        public string ChargeTypeName
        {
            get
            {
                if (!ChargeTypeId.HasValue)
                {
                    return this.Services().Localize["NoPreference"];
                }

                if (Payments == null)
                {
                    return null;
                }
                                
                var chargeType = Payments.FirstOrDefault(x => x.Id == ChargeTypeId);
				if (chargeType == null)
				{
					chargeType = Payments.FirstOrDefault();
				}

				return this.Services().Localize[chargeType.Display]; 
            }
        }

        public string Name
        {
            get
            {
                return _bookingSettings.Name;
            }
            set
            {
                if (value != _bookingSettings.Name)
                {
                    _bookingSettings.Name = value;
					RaisePropertyChanged();
                }
            }
        }

		public string Email
		{
			get
			{
				return _email;
			}
			set
			{
				_email = value;
				RaisePropertyChanged();
			}
		}

		public bool CanEditEmail
		{
			get
			{
				return !IsLinkedWithFacebook;
			}
		}

		public bool IsLinkedWithFacebook
		{
			get
			{
				return _accountService.CurrentAccount.FacebookId.HasValue();
			}
		}

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
                return CountryCode.GetCountryCodeByIndex(CountryCode.GetCountryCodeIndexByCountryISOCode(_bookingSettings.Country));
            }

            set
            {
                _bookingSettings.Country = value.CountryISOCode;
                PhoneNumber.Country = value.CountryISOCode;
                RaisePropertyChanged();
            }
        }

        public string Phone
        {
            get
            {
                return _bookingSettings.Phone;
            }
            set
            {
                if (value != _bookingSettings.Phone)
                {
                    _bookingSettings.Phone = value;
                    PhoneNumber.PhoneNumber = value;
					RaisePropertyChanged();
                }
            }
        }

		public string AccountNumber
		{
			get
			{
				return _bookingSettings.AccountNumber;
			}
			set
			{
				if (value != _bookingSettings.AccountNumber)
				{
					_bookingSettings.AccountNumber = value;
					RaisePropertyChanged();
				}
			}
		}

        public string CustomerNumber
        {
            get
            {
                return _bookingSettings.CustomerNumber;
            }
            set
            {
                if (value != _bookingSettings.CustomerNumber)
                {
                    _bookingSettings.CustomerNumber = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string PayBack
        {
            get
            {
                return _bookingSettings.PayBack;
            }
            set
            {
                if (value != _bookingSettings.PayBack)
                {
                    _bookingSettings.PayBack = value;
                    RaisePropertyChanged();
                }
            }
        }

		public ICommand SetVehiculeType
        {
            get
            {
                return this.GetCommand<int?>(id =>
                {
                    VehicleTypeId = id;
                });
            }
        }

		public ICommand NavigateToUpdatePassword
        {
            get
            {
                return this.GetCommand(() => ShowViewModel<UpdatePasswordViewModel>());
            }
        }

		public ICommand SetChargeType
        {
            get
            {
                return this.GetCommand<int?>(id =>
                {
                    ChargeTypeId = id;
                });
            }
        }

        public int? ProviderId
        {
            get
            { 
                return _bookingSettings.ProviderId;
            }
        }

		public ICommand SetCompany
        {
            get
            {
                return this.GetCommand<int?>(id =>
                {
                    _bookingSettings.ProviderId = id;
                });
            }
        }

		public ICommand SaveCommand
        {
            get
            {
                return this.GetCommand(async () => 
                {
					using (this.Services ().Message.ShowProgress ())
					{
                        if (await ValidateRideSettings())
					    {
					        try
					        {
								await _accountService.UpdateSettings(_bookingSettings, Email, _accountService.CurrentAccount.DefaultTipPercent);
					            _orderWorkflowService.SetAccountNumber(_bookingSettings.AccountNumber, _bookingSettings.CustomerNumber);
					            Close(this);
					        }
					        catch (WebServiceException ex)
					        {
					            switch (ex.ErrorCode)
					            {
					                case "AccountCharge_InvalidAccountNumber":
					                    this.Services()
					                        .Message.ShowMessage(this.Services().Localize["UpdateBookingSettingsInvalidDataTitle"],
					                            this.Services().Localize["UpdateBookingSettingsInvalidAccount"]);
					                    break;

									case "EmailAlreadyUsed":
											this.Services()
												.Message.ShowMessage(this.Services().Localize["EmailUsedMessageTitle"],
													this.Services().Localize["EmailUsedMessage"]);
										break;

					                default:
					                    this.Services()
					                        .Message.ShowMessage(this.Services().Localize["UpdateBookingSettingsInvalidDataTitle"],
					                            this.Services().Localize["UpdateBookingSettingsGenericError"]);
					                    break;
					            }
					        }
					    }
					}
                });
            }
        }

        public async Task<bool> ValidateRideSettings()
        {
			if (!EmailHelper.IsEmail(Email))
			{
				await this.Services().Message.ShowMessage(this.Services().Localize["InvalidEmailTitle"], this.Services().Localize["InvalidEmailMessage"]);
				return false;
			}

            if (string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(Phone))
            {
                await this.Services().Message.ShowMessage(this.Services().Localize["UpdateBookingSettingsInvalidDataTitle"], this.Services().Localize["UpdateBookingSettingsEmptyField"]);
                return false;
            }

            if (!PhoneNumber.IsNumberPossible())
            {
                await this.Services().Message.ShowMessage(this.Services().Localize["UpdateBookingSettingsInvalidDataTitle"],
                    string.Format(this.Services().Localize["InvalidPhoneErrorMessage"], PhoneNumber.GetPhoneExample()));
                return false;
            }

            if (ChargeTypeId == ChargeTypes.Account.Id && string.IsNullOrWhiteSpace(AccountNumber) && string.IsNullOrWhiteSpace(CustomerNumber))
            {
                await this.Services().Message.ShowMessage(this.Services().Localize["UpdateBookingSettingsInvalidDataTitle"], this.Services().Localize["UpdateBookingSettingsEmptyAccount"]);
                return false;
            }

            if (Settings.IsPayBackRegistrationFieldRequired == true && !PayBack.HasValue())
            {
                await this.Services().Message.ShowMessage(this.Services().Localize["UpdateBookingSettingsInvalidDataTitle"], this.Services().Localize["NoPayBackErrorMessage"]);
                return false;
            }

            if (PayBack.HasValue() && (PayBack.Length > 10 || !PayBack.IsNumber()))
            {
                await this.Services().Message.ShowMessage(this.Services().Localize["UpdateBookingSettingsInvalidDataTitle"], this.Services().Localize["InvalidPayBackErrorMessage"]);
                return false;
            }

            // PayBack value is set to string empty if the field is left empty by the user
            _bookingSettings.PayBack = _bookingSettings.PayBack == string.Empty ? null : _bookingSettings.PayBack;

            Phone = PhoneHelper.GetDigitsFromPhoneNumber(Phone);

            if (ChargeTypeId == ChargeTypes.Account.Id)
            {
				var creditCard = PaymentPreferences.SelectedCreditCardId == Guid.Empty
					? default(Guid?)
					: PaymentPreferences.SelectedCreditCardId;

                try
                {
                    // Validate if the charge account needs to have a card on file to be used
                    var chargeAccount = await _accountPaymentService.GetAccountCharge(AccountNumber, CustomerNumber);
                    if (chargeAccount.UseCardOnFileForPayment && creditCard == default(Guid?))
                    {
                        await this.Services().Message.ShowMessage(this.Services().Localize["UpdateBookingSettingsInvalidDataTitle"],
                            this.Services().Localize["UpdateBookingSettingsInvalidCoF"]);
                        return false;
                    }
                }
				catch
                {
                    this.Services().Message.ShowMessage(this.Services().Localize["UpdateBookingSettingsInvalidDataTitle"],
                        this.Services().Localize["UpdateBookingSettingsInvalidAccount"]).HandleErrors();
                    return false;
                }
            }

            return true;
        }
    }
}

